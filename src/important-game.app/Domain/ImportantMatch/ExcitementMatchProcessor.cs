using important_game.ui.Core;
using important_game.ui.Core.Models;
using important_game.ui.Domain.LeagueInformation;
using important_game.ui.Infrastructure.ImportantMatch;
using System.Collections.Concurrent;

namespace important_game.ui.Domain.ImportantMatch
{
    internal class ExcitementMatchProcessor(ILeagueProcessor leagueProcessor) : IExcitmentMatchProcessor
    {
        public async Task<List<ExcitementMatch>> GetUpcomingExcitementMatchesAsync(MatchImportanceOptions options)
        {
            if (!options.IsValid())
            {
                return null;
            }

            ConcurrentBag<ExcitementMatch> matches = new ConcurrentBag<ExcitementMatch>();

            List<Task> leagueTasks = new List<Task>();

            //Process all the leagues to identify the excitement match rating for each
            foreach (var configLeague in options.Leagues)
            {
                Console.WriteLine($"{configLeague.Name}");

                leagueTasks.Add(ProcessLeagueExcitmentMatchAsync(configLeague, matches));
            }

            await Task.WhenAll(leagueTasks);

            return matches.ToList();
        }

        private async Task ProcessLeagueExcitmentMatchAsync(MatchImportanceLeague configLeague, ConcurrentBag<ExcitementMatch> matches)
        {
            try
            {

                var league = await leagueProcessor.GetLeagueDataAsync(configLeague.LeagueId);

                Console.WriteLine($"Start to process {league.Name} for season {league.CurrentSeason.Name}");

                //Get upcoming features
                var upcomingFixtures = await leagueProcessor.GetUpcomingFixturesAsync(league.Id, league.CurrentSeason.Id);
                if (upcomingFixtures == null)
                {
                    Console.WriteLine($"No upcoming features to process for {league.Name}");
                    return;
                }

                //Get league standing
                var leagueTable = await leagueProcessor.GetLeagueTableAsync(league.Id, league.CurrentSeason.Id);

                var upcomingMatchesRating = await ProcessUpcomingFixturesImportantMatches(configLeague, upcomingFixtures, leagueTable);

                foreach (var match in upcomingMatchesRating)
                {
                    matches.Add(match);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private async Task<List<ExcitementMatch>> ProcessUpcomingFixturesImportantMatches(
            MatchImportanceLeague league
            , LeagueUpcomingFixtures leagueFixtures
            , LeagueStanding leagueTable)
        {
            Console.WriteLine($"Start process upcoming features for league {league.Name}");

            var matchCalculatorOptions = new ImportantMatchCalculatorOption { CompetitionRanking = league.Importance };

            var matchImportanceResult = new List<ExcitementMatch>();

            foreach (var fixture in leagueFixtures)
            {
                var matchImportance = await CalculateMatchImportanceAsync(matchCalculatorOptions, fixture, leagueTable);

                matchImportance.League = new League
                {
                    Id = league.LeagueId,
                    Name = league.Name
                };

                matchImportanceResult.Add(matchImportance);
            }

            return matchImportanceResult;
        }

        private async Task<ExcitementMatch> CalculateMatchImportanceAsync(
            ImportantMatchCalculatorOption options, UpcomingFixture fixture, LeagueStanding leagueTable)
        {
            if (fixture == null || fixture.HomeTeam == null || fixture.AwayTeam == null || leagueTable == null)
                return null;


            // 0.2×CR
            double competitionRankValue = options.CompetitionRanking * 0.2d;
            // 0.1×FN
            //fixture Number (fixtureNumber / total Fixtures)
            double fixtureValue = ((double)leagueTable.CurrentRound / (double)leagueTable.TotalRounds) * 0.1d;

            //0.2×(Form of Team A+Form of Team B / 2 )
            var homeLastFixturesData = fixture.HomeTeam.LastFixtures;
            var awayLastFixturesData = fixture.AwayTeam.LastFixtures;

            var homeLastFixturesScoreValue = CalculateTeamLastFixturesForm(homeLastFixturesData);
            var awaitLastFixturesScoreValue = CalculateTeamLastFixturesForm(awayLastFixturesData);

            double teamsLastFixtureFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * 0.22d;

            //0.18×(Goals of Team A+Goals of Team B / 2 )
            var homeGoalsFormScoreValue = CalculateTeamGoalsForm(homeLastFixturesData);
            var awayGoalsFormScoreValue = CalculateTeamGoalsForm(awayLastFixturesData);

            double teamsGoalsFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * 0.18d;

            //0.2×TPD
            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])
            var leagueTableValue = CalculateLeagueTableValue(fixture.HomeTeam, fixture.AwayTeam, leagueTable) * 0.2;

            //0.15×H2H
            double h2hLast5Games = await CalculateHeadToHeadFormAsync(fixture.HomeTeam, fixture.AwayTeam, fixture.HeadToHead);
            double h2hValue = h2hLast5Games * 0.10d;

            // 0.15×RL


            double excitementScore =
                competitionRankValue + fixtureValue +
                teamsLastFixtureFormValue + teamsGoalsFormValue +
                leagueTableValue + h2hValue;


            return new ExcitementMatch
            {
                MatchDate = fixture.MatchDate,
                HomeTeam = fixture.HomeTeam,
                AwayTeam = fixture.AwayTeam,
                ExcitementScore = excitementScore
            };
        }

        private double CalculateLeagueTableValue(Team homeTeam, Team awayTeam, LeagueStanding leagueTable)
        {
            if (homeTeam == null || awayTeam == null || leagueTable == null || leagueTable.Standings.Count == 0)
                return 0;

            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])

            var homeTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == homeTeam.Id);
            if (homeTeamPosition == null)
                return 0;

            var awayTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == awayTeam.Id);
            if (awayTeamPosition == null)
                return 0;


            var positionDiff = Math.Abs(homeTeamPosition.Position - awayTeamPosition.Position);

            return 1d - ((double)positionDiff / ((double)leagueTable.Standings.Count - 1d));

        }

        private async Task<double> CalculateHeadToHeadFormAsync(Team homeTeam, Team awayTeam, List<Fixture> fixtures)
        {
            if (fixtures == null)
                return 0;

            var homeTeamWins = fixtures.Where(c =>
                (c.HomeTeam.Id == homeTeam.Id && c.HomeTeamScore > c.AwayTeamScore)
                || (c.AwayTeam.Id == homeTeam.Id && c.AwayTeamScore > c.HomeTeamScore)
                ).Count();

            var awayTeamWins = fixtures.Where(c =>
                (c.HomeTeam.Id == awayTeam.Id && c.HomeTeamScore > c.AwayTeamScore)
                || (c.AwayTeam.Id == awayTeam.Id && c.AwayTeamScore > c.HomeTeamScore)
                ).Count();

            var difTeamWins = Math.Abs(homeTeamWins - awayTeamWins);
            difTeamWins = difTeamWins == 0 ? 3 : difTeamWins;

            var drawGames = fixtures.Where(c => c.HomeTeamScore == c.AwayTeamScore).Count();

            return ((3d / (double)difTeamWins) + (double)drawGames) / 5d;
        }

        private double CalculateTeamLastFixturesForm(TeamFixtureData teamFixtureData)
        {
            if (teamFixtureData == null)
                return 0d;

            return ((double)(teamFixtureData.Wins * 3) + (double)(teamFixtureData.Draws)) / 15d;
        }

        private object CalculateTeamGoalsForm(TeamFixtureData teamFixtureData)
        {

            if (teamFixtureData == null || teamFixtureData.AmountOfGames == 0)
                return 0;

            return ((double)teamFixtureData.GoalsAgainst + (double)teamFixtureData.GoalsFor) / (double)teamFixtureData.AmountOfGames;

        }

    }
}
