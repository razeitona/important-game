using important_game.ui.Core.Models;
using important_game.ui.Core.SofaScoreDto;
using important_game.ui.Domain.SofaScoreAPI;

namespace important_game.ui.Domain.LeagueInformation
{
    internal class SofaScoreLeagueProcessor(ISofaScoreIntegration _sofaScoreIntegration) : ILeagueProcessor
    {

        public async Task<League> GetLeagueDataAsync(int leagueId)
        {
            var tournament = await _sofaScoreIntegration.GetTournamentAsync(leagueId);

            if (tournament?.UniqueTournament == null) { return null; }

            var tournamentSeasons = await _sofaScoreIntegration.GetTournamentSeasonsAsync(tournament.UniqueTournament.Id);

            if (tournamentSeasons?.Seasons.Count == 0) { return null; }

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

            return league;
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
                if (leagueEvent.HomeTeam == null || leagueEvent.AwayTeam == null)
                    continue;

                var gameStartTime = DateTimeOffset.FromUnixTimeSeconds(leagueEvent.StartTimestamp);

                if (gameStartTime > currentDate && currentDate.AddDays(10) > gameStartTime)
                {
                    //Add upcoming fixture
                    upcomingFixtures.Add(new UpcomingFixture
                    {
                        MatchDate = gameStartTime,
                        HomeTeam = new Team
                        {
                            Id = leagueEvent.HomeTeam.Id,
                            Name = leagueEvent.HomeTeam.Name,
                            LastFixtures = await ExtractTeamLastFixtureAsync(leagueEvent.HomeTeam.Id, 5)
                        },
                        AwayTeam = new Team
                        {
                            Id = leagueEvent.AwayTeam.Id,
                            Name = leagueEvent.AwayTeam.Name,
                            LastFixtures = await ExtractTeamLastFixtureAsync(leagueEvent.AwayTeam.Id, 5)
                        },
                        HeadToHead = await GetHeadToHeadDataAsync(leagueEvent.CustomId, gameStartTime)
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

        public async Task<LeagueStanding> GetLeagueTableAsync(int leagueId, int seasonId)
        {
            var leagueTableData = await _sofaScoreIntegration.GetTournamentSeasonsTableAsync(leagueId, seasonId);
            if (leagueTableData == null || leagueTableData.Standings == null)
                return null;

            var standingData = leagueTableData.Standings.FirstOrDefault();

            if (standingData == null)
                return null;

            var leagueRoundsData = await _sofaScoreIntegration.GetTournamentSeasonRoundsAsync(leagueId, seasonId);

            if (leagueRoundsData == null)
                return null;


            var leagueStanding = new LeagueStanding()
            {
                LeagueId = leagueId,
                Standings = new List<Standing>(),
                CurrentRound = leagueRoundsData.CurrentRound.Round,
                TotalRounds = leagueRoundsData.Rounds.Count
            };

            try
            {
                foreach (var standingRow in standingData.Rows)
                {
                    leagueStanding.Standings.Add(new Standing
                    {
                        Team = new Team
                        {
                            Id = standingRow.Team.Id,
                            Name = standingRow.Team.Name,
                        },
                        Matches = standingRow.Matches!.Value,
                        Wins = standingRow.Wins!.Value,
                        Draws = standingRow.Draws!.Value,
                        Losses = standingRow.Losses!.Value,
                        GoalsFor = standingRow.ScoresFor!.Value,
                        GoalsAgainst = standingRow.ScoresAgainst!.Value,
                        Points = standingRow.Points!.Value,
                        Position = standingRow.Position!.Value,
                    });
                }
            }
            catch (Exception ex)
            {

            }

            return leagueStanding;
        }


        private async Task<List<Fixture>> GetHeadToHeadDataAsync(string ssCustomEventId, DateTimeOffset eventStartTime)
        {
            var fixtureResult = new List<Fixture>();

            var fixtureH2HData = await _sofaScoreIntegration.GetEventH2HAsync(ssCustomEventId);

            if (fixtureH2HData == null || fixtureH2HData.Events == null)
                return fixtureResult;

            foreach (var matchEvent in fixtureH2HData.Events.OrderByDescending(c => c.StartTimestamp))
            {
                if (matchEvent.Status.Code != 100)
                    continue;

                var matchStartTime = DateTimeOffset.FromUnixTimeSeconds(matchEvent.StartTimestamp);

                //Skip past events that are older than 2 years
                if (eventStartTime.AddYears(-2) > matchStartTime)
                    continue;

                fixtureResult.Add(new Fixture
                {
                    MatchDate = matchStartTime,
                    HomeTeam = new Team
                    {
                        Id = matchEvent.HomeTeam.Id,
                        Name = matchEvent.HomeTeam.Name,
                    },
                    HomeTeamScore = matchEvent.HomeScore.Current,
                    AwayTeam = new Team
                    {
                        Id = matchEvent.AwayTeam.Id,
                        Name = matchEvent.AwayTeam.Name,
                    },
                    AwayTeamScore = matchEvent.AwayScore.Current
                });


            }

            return fixtureResult;

        }

        private async Task<TeamFixtureData> ExtractTeamLastFixtureAsync(int teamId, int lastFixtureAmount)
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

        private (EventResultStatusEnum? EventResult, int GoalsFor, int GoalsAgainst) GetEventResultStatus(int teamId, SSEvent previousEvent)
        {
            var teamScore = 0;
            var oppositeTeamScore = 0;

            if (previousEvent.HomeTeam.Id == teamId)
            {
                teamScore = previousEvent.HomeScore.Current;
                oppositeTeamScore = previousEvent.AwayScore.Current;
            }
            else if (previousEvent.AwayTeam.Id == teamId)
            {
                teamScore = previousEvent.AwayScore.Current;
                oppositeTeamScore = previousEvent.HomeScore.Current;
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
