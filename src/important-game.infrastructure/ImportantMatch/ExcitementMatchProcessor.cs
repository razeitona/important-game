﻿using important_game.infrastructure.Extensions;
using important_game.infrastructure.ImportantMatch.Models;
using important_game.infrastructure.LeagueProcessors;
using System.Collections.Concurrent;

namespace important_game.infrastructure.ImportantMatch
{
    internal class ExcitementMatchProcessor(ILeagueProcessor leagueProcessor) : IExcitmentMatchProcessor
    {
        public async Task<List<ExcitementMatch>> GetUpcomingExcitementMatchesAsync(ExctimentMatchOptions options)
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

                var league = await leagueProcessor.GetLeagueDataAsync(configLeague);

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

                var upcomingMatchesRating = await ProcessUpcomingFixturesImportantMatches(league, upcomingFixtures, leagueTable);

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
            League league
            , LeagueUpcomingFixtures leagueFixtures
            , LeagueStanding leagueTable)
        {
            Console.WriteLine($"Start process upcoming features for league {league.Name}");

            var matchImportanceResult = new List<ExcitementMatch>();

            foreach (var fixture in leagueFixtures)
            {
                var matchImportance = await CalculateMatchImportanceAsync(league, fixture, leagueTable);

                if (matchImportance == null)
                    continue;

                matchImportance.League = new League
                {
                    Id = league.Id,
                    Name = league.Name,
                    PrimaryColor = league.PrimaryColor,
                    BackgroundColor = league.BackgroundColor,
                    LeagueRanking = league.LeagueRanking,
                    CurrentSeason = new LeagueSeason
                    {
                        Round = leagueTable.CurrentRound
                    }
                };

                matchImportanceResult.Add(matchImportance);
            }

            return matchImportanceResult;
        }

        private async Task<ExcitementMatch> CalculateMatchImportanceAsync(
            League league, UpcomingFixture fixture, LeagueStanding leagueTable)
        {
            if (fixture == null || fixture.HomeTeam == null || fixture.AwayTeam == null)
                return null;

            //var competitionCoef = 0.36d;
            //var rivalryCoef = 0.15d;
            //var tableRankCoef = 0.2d;
            //var fixtureCoef = 0.09d;
            //var teamFormCoef = 0.13d;
            //var teamGoalsCoef = 0.02d;
            //var h2hCoef = 0.02d;
            //var titleHolderCoef = 0.02d;

            var competitionCoef = 0.15d;
            var rivalryCoef = 0.1d;
            var fixtureCoef = 0.1d;
            var teamFormCoef = 0.25d;
            var teamGoalsCoef = 0.18d;
            var tableRankCoef = 0.1d;
            var h2hCoef = 0.1d;
            var titleHolderCoef = 0.02d;


            // 0.2×CR
            double competitionRankValue = league.LeagueRanking * competitionCoef;
            // 0.1×FN
            //fixture Number (fixtureNumber / total Fixtures)
            double fixtureValue = 1d;
            if (leagueTable != null)
            {
                fixtureValue = ((double)leagueTable.CurrentRound / (double)leagueTable.TotalRounds) * fixtureCoef;
            }

            //double fixtureValue = ((double)leagueTable.CurrentRound / (double)leagueTable.TotalRounds) * 0.2d;

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
            //double teamsGoalsFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * 0.3d;

            //double teamsFormValue = (teamsLastFixtureFormValue + teamsGoalsFormValue) * 0.3d;

            //0.2×TPD
            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])
            var leagueTableValue = CalculateLeagueTableValue(fixture.HomeTeam, fixture.AwayTeam, leagueTable) * tableRankCoef;

            //0.15×H2H
            double h2hLast5Games = await CalculateHeadToHeadFormAsync(fixture.HomeTeam, fixture.AwayTeam, fixture.HeadToHead);
            double h2hValue = h2hLast5Games * h2hCoef;

            // 0.15×T
            double titleHolderValue = CalculateTitleHolder(fixture.HomeTeam, fixture.AwayTeam, league.TitleHolder) * titleHolderCoef;

            double rivalryValue = CalculateRivalry(fixture.HomeTeam, fixture.AwayTeam, league.LeagueRanking) * rivalryCoef;


            double excitementScore =
                competitionRankValue + fixtureValue +
                teamsLastFixtureFormValue + teamsGoalsFormValue +
                //teamsFormValue +
                leagueTableValue + h2hValue
                + titleHolderValue + rivalryValue;
            return new ExcitementMatch
            {
                Id = fixture.Id,
                MatchDate = fixture.MatchDate,
                HomeTeam = fixture.HomeTeam,
                AwayTeam = fixture.AwayTeam,
                HeadToHead = fixture.HeadToHead,
                ExcitementScore = excitementScore,
                Score = new Dictionary<string, double>(){
                    { MatchDataPoint.CompetitionRank.GetDescription(), competitionRankValue/competitionCoef  },
                    { MatchDataPoint.FixtureValue.GetDescription(), fixtureValue /fixtureCoef  },
                    { MatchDataPoint.TeamsLastFixtureFormValue.GetDescription(), teamsLastFixtureFormValue /teamFormCoef  },
                    { MatchDataPoint.TeamsGoalsFormValue.GetDescription(), teamsGoalsFormValue /teamGoalsCoef  },
                    { MatchDataPoint.LeagueTableValue.GetDescription(), leagueTableValue/tableRankCoef  },
                    { MatchDataPoint.H2HValue.GetDescription(), h2hValue/h2hCoef  },
                    { MatchDataPoint.TitleHolderValue.GetDescription(), titleHolderValue/titleHolderCoef  },
                    { MatchDataPoint.RivalryValue.GetDescription(), rivalryValue/rivalryCoef  }
                }
            };
        }

        private double CalculateRivalry(Team homeTeam, Team awayTeam, double leagueRanking)
        {
            var rivalryInfo = ExctimentMatchOptions.Rivalry.Where(c =>
            (c.TeamOneId == homeTeam.Id && c.TeamTwoId == awayTeam.Id)
            ||
            (c.TeamOneId == awayTeam.Id && c.TeamTwoId == homeTeam.Id)).FirstOrDefault();

            if (rivalryInfo == null)
                return 0d;

            return rivalryInfo.Excitment;
        }

        private double CalculateTitleHolder(Team homeTeam, Team awayTeam, Team titleHolder)
        {
            if (titleHolder == null)
                return 0d;

            homeTeam.IsTitleHolder = titleHolder.Id == homeTeam.Id;
            awayTeam.IsTitleHolder = titleHolder.Id == awayTeam.Id;

            return homeTeam.IsTitleHolder || awayTeam.IsTitleHolder ? 1 : 0;
        }

        private double CalculateLeagueTableValue(Team homeTeam, Team awayTeam, LeagueStanding leagueTable)
        {
            if (homeTeam == null || awayTeam == null || leagueTable == null || leagueTable.Standings.Count == 0)
                return 0;

            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])

            var homeTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == homeTeam.Id);
            if (homeTeamPosition == null)
                return 0;

            homeTeam.Position = homeTeamPosition.Position;

            var awayTeamPosition = leagueTable.Standings.FirstOrDefault(c => c.Team.Id == awayTeam.Id);
            if (awayTeamPosition == null)
                return 0;

            if (leagueTable.CurrentRound <= 1)
                return 0.5d;

            awayTeam.Position = awayTeamPosition.Position;

            var positionDiff = (double)Math.Abs(homeTeamPosition.Position - awayTeamPosition.Position);
            var totalTeams = (double)leagueTable.Standings.Count;

            //var positionValue = 1d - ((double)positionDiff / ((double)leagueTable.Standings.Count - 1d));
            var positionValue = 1d / (1 + positionDiff / (totalTeams - 1d));


            var averageTeamPosition = ((double)homeTeamPosition.Position + (double)awayTeamPosition.Position) / 2d;

            var topBottomMatchupValue = 1d - (averageTeamPosition - 1d) / (totalTeams - 1d);

            var pointDifference = (double)Math.Abs(homeTeamPosition.Points - awayTeamPosition.Points);

            var maxPointDifference = (double)(leagueTable.TotalRounds - Math.Max(homeTeamPosition.Matches, awayTeamPosition.Matches)) * 3d;

            var pointImpactValue = 1d / (1 + pointDifference / (maxPointDifference - 1d));

            return positionValue * topBottomMatchupValue * pointImpactValue;

        }

        private async Task<double> CalculateHeadToHeadFormAsync(Team homeTeam, Team awayTeam, List<Fixture> fixtures)
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

            return 1d - ((homeTeamWins + awayTeamWins) * 3d + drawGames) / 15d;
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
