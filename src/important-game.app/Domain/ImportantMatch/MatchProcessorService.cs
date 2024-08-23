using important_game.ui.Core.Models;
using important_game.ui.Core.SofaScoreDto;
using important_game.ui.Domain.SofaScoreAPI;
using important_game.ui.Infrastructure.ImportantMatch;

namespace important_game.ui.Domain.ImportantMatch
{
    internal class MatchProcessorService(ISofaScoreIntegration _sofaScoreIntegration) : IMatchProcessorService
    {

        public async Task<List<League>> GetLeaguesAsync(params int[] sofaLeaguesIds)
        {
            var leagues = new List<League>();
            foreach (var sofaLeagueId in sofaLeaguesIds)
            {
                var tournament = await _sofaScoreIntegration.GetTournamentAsync(sofaLeagueId);

                if (tournament?.UniqueTournament == null) { continue; }

                var tournamentSeasons = await _sofaScoreIntegration.GetTournamentSeasonsAsync(tournament.UniqueTournament.Id);

                if (tournamentSeasons?.Seasons.Count == 0) { continue; }

                var currentSeason = tournamentSeasons.Seasons.First();

                var league = new League
                {
                    Id = tournament.UniqueTournament.Id,
                    Name = tournament.UniqueTournament.Name,
                    CurrentSeason = new LeagueSeason
                    {
                        Id = currentSeason.Id,
                        Name = currentSeason.Name
                    }
                };

                leagues.Add(league);
            }

            return leagues;

        }

        public async Task<LeagueUpcomingFixtures> GetUpcomingFixturesAsync(int leagueId, int seasonId)
        {
            LeagueUpcomingFixtures upcomingFixtures = new();

            var upcomingSeasonFixturesData = await _sofaScoreIntegration.GetTournamentUpcomingSeasonEventsAsync(leagueId, seasonId);

            if (upcomingSeasonFixturesData == null || upcomingSeasonFixturesData.Events.Count == 0)
                return upcomingFixtures;

            var currentDate = DateTime.UtcNow;

            foreach (var leagueEvent in upcomingSeasonFixturesData.Events)
            {
                if (!leagueEvent.StartTimestamp.HasValue)
                    continue;

                if (leagueEvent.HomeTeam == null || leagueEvent.AwayTeam == null)
                    continue;

                var gameStartTime = DateTimeOffset.FromUnixTimeSeconds(leagueEvent.StartTimestamp.Value);

                if (gameStartTime > currentDate && currentDate.AddDays(2) > gameStartTime)
                {
                    //Add upcoming fixture
                    upcomingFixtures.Add(new Fixture
                    {
                        SSEventId = leagueEvent.Id!.Value,
                        HomeTeam = new Team
                        {
                            Id = leagueEvent.HomeTeam.Id!.Value,
                            Name = leagueEvent.HomeTeam.Name,
                        },
                        AwayTeam = new Team
                        {
                            Id = leagueEvent.AwayTeam.Id!.Value,
                            Name = leagueEvent.AwayTeam.Name,
                        }
                    });

                }
                else
                {
                    //Leave, as this game is already in future
                    break;
                }

            }

            return upcomingFixtures;
        }

        public async Task<MatchImportance> CalculateMatchImportanceAsync(ImportantMatchCalculatorOption options, Fixture fixture)
        {
            if (fixture == null || fixture.HomeTeam == null || fixture.AwayTeam == null)
                return null;


            // 0.2×CR
            double competitionRankValue = options.CompetitionRanking * 0.2d;
            // 0.1×FN
            //fixture Number (fixtureNumber / total Fixtures)

            //0.2×(Form of Team A+Form of Team B / 2 )
            var homeLastFixturesData = await ExtractTeamLastFixtureFormAsync(fixture.HomeTeam.Id, 5);
            var awayLastFixturesData = await ExtractTeamLastFixtureFormAsync(fixture.AwayTeam.Id, 5);

            var homeLastFixturesScoreValue = CalculateTeamLastFixturesForm(homeLastFixturesData);
            var awaitLastFixturesScoreValue = CalculateTeamLastFixturesForm(awayLastFixturesData);

            double teamsLastFixtureFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * 0.2d;

            var homeGoalsFormScoreValue = CalculateTeamGoalsForm(homeLastFixturesData);
            var awayGoalsFormScoreValue = CalculateTeamGoalsForm(awayLastFixturesData);

            double teamsGoalsFormValue = ((homeLastFixturesScoreValue + awaitLastFixturesScoreValue) / 2d) * 0.18d;

            //0.2×TPD
            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])

            //0.15×H2H
            double h2hLast5Games = await CalculateHeadToHeadFormAsync(fixture.SSEventId);
            double h2hValue = h2hLast5Games * 0.15d;

            // 0.15×RL


            double matchImportance = competitionRankValue + teamsLastFixtureFormValue + teamsGoalsFormValue + h2hValue;


            return new MatchImportance
            {
                HomeTeam = fixture.HomeTeam,
                AwayTeam = fixture.AwayTeam,
                Importance = matchImportance
            };
        }

        private async Task<double> CalculateHeadToHeadFormAsync(int sSEventId)
        {
            var fixtureH2H = await _sofaScoreIntegration.GetEventH2HAsync(sSEventId);

            var difTeamWins = Math.Abs(fixtureH2H.TeamDuel.HomeWins.Value - fixtureH2H.TeamDuel.AwayWins.Value);
            difTeamWins = difTeamWins == 0 ? 3 : difTeamWins;


            return (3d / difTeamWins) + (double)fixtureH2H.TeamDuel.Draws.Value;
        }

        private async Task<TeamFixtureData> ExtractTeamLastFixtureFormAsync(int teamId, int lastFixtureAmount)
        {
            var teamPreviousEvents = await _sofaScoreIntegration.GetTeamPreviousEventsAsync(teamId);

            if (teamPreviousEvents == null || teamPreviousEvents.Events.Count == 0)
                return null;

            var fixtureScore = new TeamFixtureData();

            foreach (var previousEvent in teamPreviousEvents.Events.Take(lastFixtureAmount))
            {
                var eventResultData = GetEventResultStatus(teamId, previousEvent);

                if (eventResultData.EventResult == null)
                    continue;

                _ = eventResultData.EventResult switch
                {
                    EventResultStatusEnum.Win => fixtureScore.Wins += 1,
                    EventResultStatusEnum.Draw => fixtureScore.Draws += 1,
                    EventResultStatusEnum.Lost => fixtureScore.Lost += 1,
                    _ => 0,
                };

                fixtureScore.GoalsFor += eventResultData.GoalsFor;
                fixtureScore.GoalsAgainst += eventResultData.GoalsAgainst;

                fixtureScore.AmountOfGames += 1;
            }

            return fixtureScore;
        }

        private double CalculateTeamLastFixturesForm(TeamFixtureData teamFixtureData)
        {
            if (teamFixtureData == null)
                return 0d;

            return ((teamFixtureData.Wins * 3) + (teamFixtureData.Draws)) / 15d;
        }

        private object CalculateTeamGoalsForm(TeamFixtureData teamFixtureData)
        {

            if (teamFixtureData == null || teamFixtureData.AmountOfGames == 0)
                return 0;

            return (teamFixtureData.GoalsAgainst + teamFixtureData.GoalsFor) / teamFixtureData.AmountOfGames;

        }

        private (EventResultStatusEnum? EventResult, int GoalsFor, int GoalsAgainst) GetEventResultStatus(int teamId, SSEvent previousEvent)
        {
            var teamScore = 0;
            var oppositeTeamScore = 0;

            if (!previousEvent.HomeScore.Current.HasValue || !previousEvent.AwayScore.Current.HasValue)
                return (null, 0, 0);

            if (previousEvent.HomeTeam.Id == teamId)
            {
                teamScore = previousEvent.HomeScore.Current.Value;
                oppositeTeamScore = previousEvent.AwayScore.Current.Value;
            }
            else if (previousEvent.AwayTeam.Id == teamId)
            {
                teamScore = previousEvent.AwayScore.Current.Value;
                oppositeTeamScore = previousEvent.HomeScore.Current.Value;
            }
            else
            {
                return (null, 0, 0);
            }

            var eventScore = EventResultStatusEnum.Draw;

            if (teamScore > oppositeTeamScore)
                eventScore = EventResultStatusEnum.Win;
            else if (oppositeTeamScore > teamScore)
                eventScore = EventResultStatusEnum.Lost;

            return (eventScore, teamScore, oppositeTeamScore);
        }
    }
}
