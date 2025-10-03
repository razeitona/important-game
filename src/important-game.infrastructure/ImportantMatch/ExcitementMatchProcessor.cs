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
        , IExctimentMatchRepository matchRepository, ITelegramBot telegramBot) : IExcitmentMatchProcessor
    {
        private const double CompetitionCoef = 0.15d;
        private const double FixtureCoef = 0.10d;
        private const double TeamFormCoef = 0.10d;
        private const double TeamGoalsCoef = 0.15d;
        private const double TableRankCoef = 0.15d;
        private const double HeadToHeadCoef = 0.10d;
        private const double TitleHolderCoef = 0.10d;
        private const double RivalryCoef = 0.15d;

        private const double LateCompetitionCoef = 0.05d;
        private const double LateFixtureCoef = 0.25d;
        private const double LateTeamFormCoef = 0.05d;
        private const double LateTeamGoalsCoef = 0.2d;
        private const double LateTableRankCoef = 0.33d;
        private const double LateHeadToHeadCoef = 0.05d;
        private const double LateTitleHolderCoef = 0.05d;
        private const double LateRivalryCoef = 0.02d;

        // Limit concurrent SofaScore calls to avoid throttling by the upstream API.
        private const int MaxConcurrentLeagueRequests = 1;
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
            var competitionRankValue = league.Ranking;
            var fixtureValue = CalculateFixtureValue(leagueTable);
            var homeLastFixturesScoreValue = CalculateTeamLastFixturesForm(fixture.HomeTeam.LastFixtures);
            var awayLastFixturesScoreValue = CalculateTeamLastFixturesForm(fixture.AwayTeam.LastFixtures);
            var teamsLastFixtureFormValue = (homeLastFixturesScoreValue + awayLastFixturesScoreValue) / 2d;
            var homeGoalsFormScoreValue = CalculateTeamGoalsForm(fixture.HomeTeam.LastFixtures);
            var awayGoalsFormScoreValue = CalculateTeamGoalsForm(fixture.AwayTeam.LastFixtures);
            var teamsGoalsFormValue = homeGoalsFormScoreValue + awayGoalsFormScoreValue; // Changed from average to sum
            var leagueTableValue = CalculateLeagueTableValue(fixture.HomeTeam, fixture.AwayTeam, leagueTable);
            var h2hValue = await CalculateHeadToHeadFormAsync(fixture.HomeTeam, fixture.AwayTeam, fixture.HeadToHead);
            var titleHolderValue = CalculateTitleHolder(fixture.HomeTeam, fixture.AwayTeam, league.TitleHolder);
            var rivalryValue = rivalry?.RivarlyValue ?? 0d;

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
                CompetitionScore = competitionRankValue,
                FixtureScore = fixtureValue,
                FormScore = teamsLastFixtureFormValue,
                GoalsScore = teamsGoalsFormValue,
                CompetitionStandingScore = leagueTableValue,
                HeadToHeadScore = h2hValue,
                TitleHolderScore = titleHolderValue,
                RivalryScore = rivalryValue,
                HomeForm = PrepareTeamForm(fixture.HomeTeam),
                AwayForm = PrepareTeamForm(fixture.AwayTeam),
                MatchStatus = MatchStatus.Upcoming
            };

            var isLateStage = ((double)leagueTable.CurrentRound / (double)leagueTable.TotalRounds) > 0.8d 
                && leagueTable.TotalRounds > leagueTable.Standings.Count;

            double competitionCoef = isLateStage ? LateCompetitionCoef : CompetitionCoef;
            double rivalryCoef = isLateStage ? LateRivalryCoef : RivalryCoef;
            double titleHolderCoef = isLateStage ? LateTitleHolderCoef : TitleHolderCoef;
            double teamFormCoef = isLateStage ? LateTeamFormCoef : TeamFormCoef;
            double teamGoalsCoef = isLateStage ? LateTeamGoalsCoef : TeamGoalsCoef;
            double h2hCoef = isLateStage ? LateHeadToHeadCoef : HeadToHeadCoef;
            double tableRankCoef = isLateStage ? LateTableRankCoef : TableRankCoef;
            double fixtureCoef = isLateStage ? LateFixtureCoef : FixtureCoef;

            match.ExcitmentScore =
                match.CompetitionScore * competitionCoef +
                match.FixtureScore * fixtureCoef +
                match.FormScore * teamFormCoef +
                match.GoalsScore * teamGoalsCoef +
                match.CompetitionStandingScore * tableRankCoef +
                match.HeadToHeadScore * h2hCoef +
                match.TitleHolderScore * titleHolderCoef +
                match.RivalryScore * rivalryCoef;

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
            if (homeTeam == null || awayTeam == null || leagueTable == null || leagueTable.Standings == null || leagueTable.Standings.Count == 0)
                return 0d;

            if (leagueTable.CurrentRound <= 1)
                return 0.5d;

            var homeTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == homeTeam.Id);
            if (homeTeamPosition == null)
                return 0d;

            homeTeam.Position = homeTeamPosition.Position;

            var awayTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == awayTeam.Id);
            if (awayTeamPosition == null)
                return 0d;

            awayTeam.Position = awayTeamPosition.Position;

            var positionDiff = Math.Abs(homeTeamPosition.Position - awayTeamPosition.Position) - 1d;
            var totalTeams = (double)leagueTable.Standings.Count;

            var positionValue = 1d / (1d + (positionDiff / (totalTeams - 1d)));
            var averageTeamPosition = (homeTeamPosition.Position + awayTeamPosition.Position) / 2d;
            var topBottomMatchupValue = 1d - (averageTeamPosition - 1d) / (totalTeams - 1d);

            var pointDifference = Math.Abs(homeTeamPosition.Points - awayTeamPosition.Points);
            var maxPointDifference = (leagueTable.TotalRounds - Math.Max(homeTeamPosition.Matches, awayTeamPosition.Matches)) * 3d;
            var pointImpactValue = 1d / (1d + (pointDifference / (maxPointDifference - 1d)));

            return positionValue * topBottomMatchupValue * pointImpactValue;

        }

        private async Task<double> CalculateHeadToHeadFormAsync(TeamInfo homeTeam, TeamInfo awayTeam, List<Fixture> fixtures)
        {
            if (fixtures == null || fixtures.Count == 0)
                return 0.5d;

            var homeTeamWins = fixtures.Count(c =>
                (c.HomeTeam.Id == homeTeam.Id && c.HomeTeamScore > c.AwayTeamScore) ||
                (c.AwayTeam.Id == homeTeam.Id && c.AwayTeamScore > c.HomeTeamScore));

            homeTeam.H2hWins = homeTeamWins;
            homeTeam.Goals = fixtures.Where(c => c.HomeTeam.Id == homeTeam.Id).Sum(c => c.HomeTeamScore) +
                fixtures.Where(c => c.AwayTeam.Id == homeTeam.Id).Sum(c => c.AwayTeamScore);

            var awayTeamWins = fixtures.Count(c =>
                (c.HomeTeam.Id == awayTeam.Id && c.HomeTeamScore > c.AwayTeamScore) ||
                (c.AwayTeam.Id == awayTeam.Id && c.AwayTeamScore > c.HomeTeamScore));

            awayTeam.H2hWins = awayTeamWins;
            awayTeam.Goals = fixtures.Where(c => c.HomeTeam.Id == awayTeam.Id).Sum(c => c.HomeTeamScore) +
                fixtures.Where(c => c.AwayTeam.Id == awayTeam.Id).Sum(c => c.AwayTeamScore);

            var drawGames = fixtures.Count(c => c.HomeTeamScore == c.AwayTeamScore);

            var value = ((homeTeamWins + awayTeamWins) * 3d + drawGames) / 15d;
            return value > 1d ? 1d : value;
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

            return (double)teamFixtureData.GoalsFor / (double)teamFixtureData.AmountOfGames * 2.0;
        }

        private double CalculateFixtureValue(LeagueStanding leagueStanding)
        {
            if (leagueStanding == null || leagueStanding.TotalRounds == 0)
                return 0d;

            return ((double)leagueStanding.CurrentRound / (double)leagueStanding.TotalRounds);
        }
    }
}