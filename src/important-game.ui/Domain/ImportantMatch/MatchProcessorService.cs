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
            double homeLastFixtures = await CalculateTeamLastFeaturesFormAsync(fixture.HomeTeam.Id, 5);
            double awayLastFixtures = await CalculateTeamLastFeaturesFormAsync(fixture.AwayTeam.Id, 5);
            double teamsLastFixtureFormValue = ((homeLastFixtures + awayLastFixtures) / 2d) * 0.2d;

            //0.2×TPD
            //table position difference ( 1 - [(teamA-teamB)/totalTeams-1])

            //0.15×H2H
            double h2hLast5Games = await CalculateHeadToHeadFormAsync(fixture.SSEventId);
            double h2hValue = h2hLast5Games * 0.15d;

            // 0.15×RL


            double matchImportance = competitionRankValue + teamsLastFixtureFormValue + h2hValue;


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

        private async Task<double> CalculateTeamLastFeaturesFormAsync(int teamId, int lastFixtureAmount)
        {
            var teamPreviousEvents = await _sofaScoreIntegration.GetTeamPreviousEventsAsync(teamId);

            if (teamPreviousEvents == null || teamPreviousEvents.Events.Count == 0)
                return 0d;

            double formLastGames = 0d;
            foreach (var previousEvent in teamPreviousEvents.Events.Take(lastFixtureAmount))
            {
                var eventResult = GetEventResultStatus(teamId, previousEvent);

                formLastGames += eventResult switch
                {
                    EventResultStatusEnum.Win => 3,
                    EventResultStatusEnum.Draw => 1,
                    EventResultStatusEnum.Lost => 0,
                    _ => 0,
                };

            }

            return formLastGames / 15d;
        }

        private EventResultStatusEnum? GetEventResultStatus(int teamId, SSEvent previousEvent)
        {
            var teamScore = 0;
            var oppositeTeamScore = 0;

            if (!previousEvent.HomeScore.Current.HasValue || !previousEvent.AwayScore.Current.HasValue)
                return null;

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
                return null;
            }

            if (teamScore > oppositeTeamScore)
                return EventResultStatusEnum.Win;
            else if (oppositeTeamScore > teamScore)
                return EventResultStatusEnum.Lost;

            return EventResultStatusEnum.Draw;
        }
    }
}
