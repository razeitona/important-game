using System.Globalization;
using System.Threading;
using important_game.infrastructure.ImportantMatch.Data;
using important_game.infrastructure.ImportantMatch.Data.Entities;
using important_game.infrastructure.ImportantMatch.Models.Processors;
using important_game.infrastructure.LeagueProcessors;
using important_game.infrastructure.Telegram;

namespace important_game.infrastructure.ImportantMatch
{
    internal class ExcitementMatchProcessor(ILeagueProcessor leagueProcessor
        , IExctimentMatchRepository matchRepository, TelegramBot telegramBot) : IExcitmentMatchProcessor
    {
        // Limit concurrent SofaScore calls to avoid throttling by the upstream API.
        private const int MaxConcurrentLeagueRequests = 3;
        private readonly SemaphoreSlim _repositoryLock = new(1, 1);

        public async Task CalculateUpcomingMatchsExcitment()
        {

            var competitions = await matchRepository.GetActiveCompetitionsAsync();

            if (competitions == null || competitions.Count == 0)
            {
                return;
            }

            using var throttler = new SemaphoreSlim(MaxConcurrentLeagueRequests, MaxConcurrentLeagueRequests);

            var processingTasks = competitions
                .Select(competition => ProcessCompetitionAsync(competition, throttler))
                .ToList();

            await Task.WhenAll(processingTasks);
        }

        private async Task ProcessCompetitionAsync(Competition competition, SemaphoreSlim throttler)
        {
            await throttler.WaitAsync();

            try
            {
                var leagueInfo = await GetLeagueDataInfoAsync(competition);

                if (leagueInfo == null)
                {
                    return;
                }

                var activeMatches = await WithRepositoryLock(() => matchRepository.GetCompetitionActiveMatchesAsync(competition.Id));

                await ExtractUpcomingMatchesAsync(leagueInfo, activeMatches);
            }
            finally
            {
                throttler.Release();
            }
        }

        private async Task<TResult> WithRepositoryLock<TResult>(Func<Task<TResult>> action)
        {
            await _repositoryLock.WaitAsync();

            try
            {
                return await action();
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        private async Task WithRepositoryLock(Func<Task> action)
        {
            await _repositoryLock.WaitAsync();

            try
            {
                await action();
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        }

        private async Task<League?> GetLeagueDataInfoAsync(Competition competition)
        {
            var league = await leagueProcessor.GetLeagueDataAsync(competition.Id);

            if (league == null)
                return league;

            //Validate if it's to update Competition info or not
            if (league.Name != competition.Name || competition.TitleHolderTeamId != (league.TitleHolder?.Id ?? null))
            {
                var updatedCompetition = new Competition
                {
                    Id = competition.Id,
                    Name = league.Name,
                    TitleHolderTeamId = league.TitleHolder?.Id ?? null,
                };

                await WithRepositoryLock(() => matchRepository.SaveCompetitionAsync(updatedCompetition));
            }

            league.Ranking = competition.LeagueRanking;
            return league;
        }

        private async Task ExtractUpcomingMatchesAsync(League league, List<Match> activeMatches)
        {
            try
            {
                //Get upcoming features
                var sourceUpcomingMatches = await leagueProcessor.GetUpcomingMatchesAsync(league.Id, league.CurrentSeason.Id);
                if (sourceUpcomingMatches == null)
                {
                    return;
                }

                //Get league standing
                var leagueTable = await leagueProcessor.GetLeagueTableAsync(league.Id, league.CurrentSeason.Id);

                await ProcessUpcomingFixturesImportantMatches(league, sourceUpcomingMatches, leagueTable, activeMatches);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private async Task ProcessUpcomingFixturesImportantMatches(League league
            , LeagueUpcomingFixtures leagueFixtures, LeagueStanding leagueTable, List<Match> activeMatches)
        {

            var activeMatchesDict = activeMatches.ToDictionary(c => c.Id, c => c);
            var rivalryCache = new Dictionary<(int, int), Rivalry?>();

            var validationDate = DateTime.UtcNow.AddDays(-1);

            //Loop fixtures
            foreach (var fixture in leagueFixtures)
            {
                if (fixture == null || fixture.HomeTeam == null || fixture.AwayTeam == null)
                    continue;

                //Validate if match was updated recently
                if (activeMatchesDict.TryGetValue(fixture.Id, out Match? activeMatch))
                {
                    if (activeMatch.UpdatedDateUTC > validationDate)
                        continue;
                }


                var cacheKey = CreateRivalryCacheKey(fixture.HomeTeam.Id, fixture.AwayTeam.Id);

                if (!rivalryCache.TryGetValue(cacheKey, out var rivalry))
                {
                    rivalry = await WithRepositoryLock(() => matchRepository.GetRivalryByTeamIdAsync(fixture.HomeTeam.Id, fixture.AwayTeam.Id));
                    rivalryCache[cacheKey] = rivalry;
                }

                var match = await CalculateMatchImportanceAsync(league, fixture, leagueTable, rivalry);

                if (match.MatchDateUTC.Date == DateTime.UtcNow.Date)
                    _ = telegramBot.SendMessageAsync(ProcessGameMessage(match));
            }
        }

        private static (int FirstTeamId, int SecondTeamId) CreateRivalryCacheKey(int teamOneId, int teamTwoId)
        {
            return teamOneId <= teamTwoId ? (teamOneId, teamTwoId) : (teamTwoId, teamOneId);
        }

        private string ProcessGameMessage(Match match)
        {
            var homeTeamName = match.HomeTeam?.Name ?? "Home team";
            var awayTeamName = match.AwayTeam?.Name ?? "Away team";
            var leagueName = match.Competition?.Name;
            var kickOffTime = match.MatchDateUTC.ToString("ddd, MMM dd yyyy HH:mm", CultureInfo.InvariantCulture);
            var excitementScore = Math.Round(match.ExcitmentScore * 100, 0);

            var statusLine = match.MatchStatus switch
            {
                MatchStatus.Live => $"Score: {match.HomeScore}-{match.AwayScore} (live)",
                MatchStatus.Finished => $"Final score: {match.HomeScore}-{match.AwayScore}",
                _ => $"Kick-off: {kickOffTime} UTC"
            };

            var lines = new List<string?>
            {
                "MATCH TO WATCH ALERT",
                leagueName != null ? $"Competition: {leagueName}" : null,
                $"{homeTeamName} vs {awayTeamName}",
                statusLine,
                $"Excitement Score: {excitementScore}/100",
                $"Full breakdown: https://matchtowatch.net/match/{match.Id}"
            };

            return string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        }

        private async Task<Match> CalculateMatchImportanceAsync(League league, UpcomingFixture fixture
            , LeagueStanding leagueTable, Rivalry? rivalry)
        {
            //var competitionCoef = 0.36d;
            //var rivalryCoef = 0.15d;
            //var tableRankCoef = 0.2d;
            //var fixtureCoef = 0.09d;
            //var teamFormCoef = 0.13d;
            //var teamGoalsCoef = 0.02d;
            //var h2hCoef = 0.02d;
            //var titleHolderCoef = 0.02d;

            //var competitionCoef = 0.15d;
            //var rivalryCoef = 0.1d;
            //var fixtureCoef = 0.1d;
            //var teamFormCoef = 0.25d;
            //var teamGoalsCoef = 0.18d;
            //var tableRankCoef = 0.1d;
            //var h2hCoef = 0.1d;
            //var titleHolderCoef = 0.02d;

            var competitionCoef = 0.15d;
            var rivalryCoef = 0.15d;
            var titleHolderCoef = 0.10d;
            var teamFormCoef = 0.10d;
            var teamGoalsCoef = 0.15d;
            var h2hCoef = 0.10d;
            var tableRankCoef = 0.15d;
            var fixtureCoef = 0.10d;

            if (((double)leagueTable.CurrentRound / (double)leagueTable.TotalRounds) > 0.8d && leagueTable.TotalRounds > leagueTable.Standings.Count)
            {
                fixtureCoef = 0.25d;
                tableRankCoef = 0.33d;
                teamGoalsCoef = 0.2d;
                teamFormCoef = 0.05d;
                rivalryCoef = 0.02d;
                titleHolderCoef = 0.05d;
                competitionCoef = 0.05d;
                h2hCoef = 0.05d;
            }

            // 0.2×CR
            double competitionRankValue = league.Ranking * competitionCoef;
            // 0.1×FN
            //fixture Number (fixtureNumber / total Fixtures)
            double fixtureValue = 1d * fixtureCoef;
            if (leagueTable != null)
            {
                fixtureValue = ((double)leagueTable.CurrentRound / (double)leagueTable.TotalRounds) * fixtureCoef;
            }

            //0.2×(Form of Team A+Form of Team B / 2 )
            var homeLastFixturesData = fixture.HomeTeam.LastFixtures;
            var awayLastFixturesData = fixture.AwayTeam.LastFixtures;

            var homeLastFixturesScoreValue = CalculateTeamLastFixturesForm(homeLastFixturesData);
            var awaitLastFixturesScoreValue = CalculateTeamLastFixturesForm(awayLastFixturesData);

            double teamsLastFixtureFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * teamFormCoef;
            //double teamsLastFixtureFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * 0.7d;

            //0.18×(Goals of Team A+Goals of Team B / 2 )
            var homeGoalsFormScoreValue = CalculateTeamGoalsForm(homeLastFixturesData);
            var awayGoalsFormScoreValue = CalculateTeamGoalsForm(awayLastFixturesData);
            double teamsGoalsFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * teamGoalsCoef;

            //0.2×TPD
            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])
            var leagueTableValue = CalculateLeagueTableValue(fixture.HomeTeam, fixture.AwayTeam, leagueTable) * tableRankCoef;

            //0.15×H2H
            double h2hLast5Games = await CalculateHeadToHeadFormAsync(fixture.HomeTeam, fixture.AwayTeam, fixture.HeadToHead);
            double h2hValue = h2hLast5Games * h2hCoef;

            // 0.15×T
            double titleHolderValue = CalculateTitleHolder(fixture.HomeTeam, fixture.AwayTeam, league.TitleHolder) * titleHolderCoef;

            var rivalryData = (rivalry?.RivarlyValue ?? 0d);
            double rivalryValue = rivalryData * rivalryCoef;

            if (rivalryData > 0.9d)
            {
                //fixtureValue = 1 * fixtureCoef;
            }

            double excitementScore =
                competitionRankValue + fixtureValue +
                teamsLastFixtureFormValue + teamsGoalsFormValue +
                //teamsFormValue +
                leagueTableValue + h2hValue
                + titleHolderValue + rivalryValue;


            var homeTeam = await InsertTeamInfo(fixture.HomeTeam);
            var awayTeam = await InsertTeamInfo(fixture.AwayTeam);

            var match = new Match
            {
                Id = fixture.Id,
                MatchDateUTC = fixture.MatchDate.UtcDateTime,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                HomeTeamPosition = fixture.HomeTeam.Position,
                AwayTeamPosition = fixture.AwayTeam.Position,
                CompetitionId = league.Id,
                UpdatedDateUTC = DateTime.UtcNow,
                ExcitmentScore = excitementScore,
                CompetitionScore = competitionRankValue / competitionCoef,
                FixtureScore = fixtureValue / fixtureCoef,
                FormScore = teamsLastFixtureFormValue / teamFormCoef,
                GoalsScore = teamsGoalsFormValue / teamGoalsCoef,
                CompetitionStandingScore = leagueTableValue / tableRankCoef,
                HeadToHeadScore = h2hValue / h2hCoef,
                TitleHolderScore = titleHolderValue / titleHolderCoef,
                RivalryScore = rivalryValue / rivalryCoef,
                HomeForm = PrepareTeamForm(fixture.HomeTeam),
                AwayForm = PrepareTeamForm(fixture.AwayTeam),
                MatchStatus = MatchStatus.Upcoming
            };

            await WithRepositoryLock(() => matchRepository.SaveMatchAsync(match));

            await ProcessHeadToHeadMatches(match.Id, fixture.HeadToHead);

            return match;

        }

        private string PrepareTeamForm(TeamInfo teamInfo)
        {
            return string.Join(",", teamInfo.LastFixtures.FixtureResult.Select(c => (int)c).ToList());
        }

        private async Task ProcessHeadToHeadMatches(int id, List<Fixture> headToHeadFixtures)
        {

            List<Headtohead> headToHeadMatches = new();

            foreach (var fixture in headToHeadFixtures)
            {
                headToHeadMatches.Add(new Headtohead
                {
                    MatchId = id,
                    HomeTeamId = fixture.HomeTeam.Id,
                    AwayTeamId = fixture.AwayTeam.Id,
                    MatchDateUTC = fixture.MatchDate.UtcDateTime,
                    HomeTeamScore = fixture.HomeTeamScore,
                    AwayTeamScore = fixture.AwayTeamScore
                });
            }

            await WithRepositoryLock(() => matchRepository.SaveHeadToHeadMatchesAsync(headToHeadMatches));
        }

        private async Task<Team> InsertTeamInfo(TeamInfo teamInfo)
        {
            var team = new Team
            {
                Id = teamInfo.Id,
                Name = teamInfo.Name,
            };

            return await WithRepositoryLock(() => matchRepository.SaveTeamAsync(team));
        }

        private double CalculateTitleHolder(TeamInfo homeTeam, TeamInfo awayTeam, TeamTitleHolder titleHolder)
        {
            if (titleHolder == null)
                return 0d;

            homeTeam.IsTitleHolder = titleHolder.Id == homeTeam.Id;
            awayTeam.IsTitleHolder = titleHolder.Id == awayTeam.Id;

            return homeTeam.IsTitleHolder || awayTeam.IsTitleHolder ? 1 : 0;
        }

        private double CalculateLeagueTableValue(TeamInfo homeTeam, TeamInfo awayTeam, LeagueStanding leagueTable)
        {
            if (homeTeam == null || awayTeam == null || leagueTable == null || leagueTable.Standings.Count == 0)
                return 0d;

            if (leagueTable.CurrentRound <= 1)
                return 0.5d;

            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])

            var homeTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == homeTeam.Id);
            if (homeTeamPosition == null)
                return 0d;

            homeTeam.Position = homeTeamPosition.Position;

            var awayTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == awayTeam.Id);
            if (awayTeamPosition == null)
                return 0d;

            awayTeam.Position = awayTeamPosition.Position;

            var positionDiff = (double)Math.Abs(homeTeamPosition.Position - awayTeamPosition.Position) - 1;
            var totalTeams = (double)leagueTable.Standings.Count;

            //var positionValue = 1d - ((double)positionDiff / ((double)leagueTable.Standings.Count - 1d));
            var positionValue = 1d / (1 + (positionDiff / (totalTeams - 1d)));


            var averageTeamPosition = ((double)homeTeamPosition.Position + (double)awayTeamPosition.Position) / 2d;

            var topBottomMatchupValue = 1d - (averageTeamPosition - 1d) / (totalTeams - 1d);

            var pointDifference = (double)Math.Abs(homeTeamPosition.Points - awayTeamPosition.Points);

            var maxPointDifference = (double)(leagueTable.TotalRounds - Math.Max(homeTeamPosition.Matches, awayTeamPosition.Matches)) * 3d;

            var pointImpactValue = 1d / (1 + (pointDifference / (maxPointDifference - 1d)));

            return positionValue * topBottomMatchupValue * pointImpactValue;

        }

        private async Task<double> CalculateHeadToHeadFormAsync(TeamInfo homeTeam, TeamInfo awayTeam, List<Fixture> fixtures)
        {
            if (fixtures == null)
                return 0;

            if (fixtures.Count == 0)
                return 0.5d;

            var homeTeamWins = fixtures.Where(c =>
                c.HomeTeam.Id == homeTeam.Id && c.HomeTeamScore > c.AwayTeamScore
                || c.AwayTeam.Id == homeTeam.Id && c.AwayTeamScore > c.HomeTeamScore
                ).Count();

            homeTeam.H2hWins = homeTeamWins;
            homeTeam.Goals = fixtures.Where(c => c.HomeTeam.Id == homeTeam.Id).Sum(c => c.HomeTeamScore) +
                fixtures.Where(c => c.AwayTeam.Id == homeTeam.Id).Sum(c => c.AwayTeamScore);

            var awayTeamWins = fixtures.Where(c =>
                c.HomeTeam.Id == awayTeam.Id && c.HomeTeamScore > c.AwayTeamScore
                || c.AwayTeam.Id == awayTeam.Id && c.AwayTeamScore > c.HomeTeamScore
                ).Count();

            awayTeam.H2hWins = awayTeamWins;
            awayTeam.Goals = fixtures.Where(c => c.HomeTeam.Id == awayTeam.Id).Sum(c => c.HomeTeamScore) +
                fixtures.Where(c => c.AwayTeam.Id == awayTeam.Id).Sum(c => c.AwayTeamScore);


            var difTeamWins = Math.Abs(homeTeamWins - awayTeamWins);
            difTeamWins = difTeamWins == 0 ? 3 : difTeamWins;

            var drawGames = (double)fixtures.Where(c => c.HomeTeamScore == c.AwayTeamScore).Count();

            var h2hValue = (((homeTeamWins + awayTeamWins) * 3d) + drawGames) / 15d;

            if (h2hValue > 1d)
                h2hValue = 1d;

            return h2hValue;
            //return ((3d / (double)difTeamWins) + (double)drawGames) / 5d;
        }

        private double CalculateTeamLastFixturesForm(TeamFixtureData teamFixtureData)
        {
            if (teamFixtureData == null)
                return 0d;

            return ((double)(teamFixtureData.Wins * 3) + (double)teamFixtureData.Draws) / 15d;
        }

        private double CalculateTeamGoalsForm(TeamFixtureData teamFixtureData)
        {

            if (teamFixtureData == null || teamFixtureData.AmountOfGames == 0)
                return 0;

            return (double)teamFixtureData.GoalsFor / (double)teamFixtureData.AmountOfGames;
            //return ((double)teamFixtureData.GoalsAgainst + (double)teamFixtureData.GoalsFor) / (double)teamFixtureData.AmountOfGames;

        }

    }
}

