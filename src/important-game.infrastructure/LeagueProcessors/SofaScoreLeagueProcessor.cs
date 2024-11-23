using important_game.infrastructure.ImportantMatch.Models;
using important_game.infrastructure.ImportantMatch.Models.Processors;
using important_game.infrastructure.SofaScoreAPI;
using important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto;

namespace important_game.infrastructure.LeagueProcessors
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

            var uniqueTournament = tournament.UniqueTournament;

            var league = new League
            {
                Id = uniqueTournament.Id,
                Name = uniqueTournament.Name,
                CurrentSeason = new LeagueSeason
                {
                    Id = currentSeason.Id,
                    Name = currentSeason.Name,
                },
                TitleHolder = new TeamTitleHolder
                {
                    Id = uniqueTournament.TitleHolder.Id,
                    Name = uniqueTournament.TitleHolder.Name
                }
            };

            return league;
        }

        public async Task<LeagueUpcomingFixtures> GetUpcomingMatchesAsync(int leagueId, int seasonId)
        {
            LeagueUpcomingFixtures upcomingFixtures = new();

            var upcomingSeasonFixturesData = await _sofaScoreIntegration.GetTournamentUpcomingSeasonEventsAsync(leagueId, seasonId);

            if (upcomingSeasonFixturesData?.Events == null || upcomingSeasonFixturesData.Events.Count == 0)
                return upcomingFixtures;

            var currentDate = DateTime.UtcNow;

            foreach (var leagueEvent in upcomingSeasonFixturesData.Events)
            {
                if (leagueEvent.HomeTeam == null || leagueEvent.AwayTeam == null)
                    continue;

                var gameStartTime = DateTimeOffset.FromUnixTimeSeconds(leagueEvent.StartTimestamp);

                //Get Events that start in 5 days
                if (gameStartTime < currentDate.AddDays(5) && gameStartTime > currentDate.AddHours(-3))
                {
                    //Add upcoming fixture
                    upcomingFixtures.Add(new UpcomingFixture
                    {
                        Id = leagueEvent.Id,
                        MatchDate = gameStartTime,
                        HomeTeam = new TeamInfo
                        {
                            Id = leagueEvent.HomeTeam.Id,
                            Name = leagueEvent.HomeTeam.Name,
                            LastFixtures = await ExtractTeamLastFixtureAsync(leagueEvent.HomeTeam.Id, 5)
                        },
                        AwayTeam = new TeamInfo
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
                    //Leave, as this game is already in future or past
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

            var leagueStanding = new LeagueStanding()
            {
                LeagueId = leagueId,
                Standings = new List<Standing>(),
                CurrentRound = leagueRoundsData?.CurrentRound?.Round??1,
                TotalRounds = leagueRoundsData?.Rounds?.Count??1
            };

            try
            {
                foreach (var standingRow in standingData.Rows)
                {
                    leagueStanding.Standings.Add(new Standing
                    {
                        Team = new TeamInfo
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
                if (eventStartTime.AddYears(-2) >= matchStartTime)
                    continue;

                fixtureResult.Add(new Fixture
                {
                    Id = matchEvent.Id,
                    MatchDate = matchStartTime,
                    HomeTeam = new TeamInfo
                    {
                        Id = matchEvent.HomeTeam.Id,
                        Name = matchEvent.HomeTeam.Name,
                    },
                    HomeTeamScore = matchEvent.HomeScore.Current,
                    AwayTeam = new TeamInfo
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

            //extract x amount of last features but sort by oldest
            foreach (var previousEvent in teamPreviousEvents.Events
                .OrderByDescending(c => c.StartTimestamp)
                .Take(lastFixtureAmount)
                .OrderBy(c => c.StartTimestamp))
            {
                var eventResultData = GetEventResultStatus(teamId, previousEvent);

                if (eventResultData.EventResult == null)
                    continue;

                switch (eventResultData.EventResult)
                {
                    case EventResultStatusEnum.Win:
                        fixtureScore.FixtureResult.Add(MatchResultType.Win);
                        fixtureScore.Wins += 1;
                        break;
                    case EventResultStatusEnum.Draw:
                        fixtureScore.FixtureResult.Add(MatchResultType.Draw);
                        fixtureScore.Draws += 1;
                        break;
                    case EventResultStatusEnum.Lost:
                        fixtureScore.FixtureResult.Add(MatchResultType.Lost);
                        fixtureScore.Lost += 1;
                        break;

                    default:
                        break;
                }

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

        public async Task<EventStatistics?> GetEventStatisticsAsync(string eventId)
        {
            var ssStatistics = await _sofaScoreIntegration.GetEventStatisticsAsync(eventId);

            if (ssStatistics is null || ssStatistics.Statistics is null)
                return default;

            var eventStatistics = ProcessEventStatistics(ssStatistics);

            return eventStatistics;
        }

        public async Task<EventInfo> GetEventInformationAsync(string eventId)
        {
            var ssEventInfo = await _sofaScoreIntegration.GetEventInformationAsync(eventId);

            if (ssEventInfo is null || ssEventInfo.EventData is null)
                return default;

            var ssEventData = ssEventInfo.EventData;

            return new EventInfo
            {
                Id = ssEventData.Id,
                AwayTeam = new TeamInfo
                {
                    Id = ssEventData.AwayTeam.Id,
                },
                AwayTeamScore = ssEventData.AwayScore.Current,
                HomeTeam = new TeamInfo
                {
                    Id = ssEventData.HomeTeam.Id,
                },
                HomeTeamScore = ssEventData.HomeScore.Current,
                Status = new EventStatus
                {
                    Status = ssEventData.Status.Type,
                    StatusCode = (EventMatchStatus)(ssEventData.Status.Code ?? 0),
                    Period = ssEventData.LastPeriod,
                    MatchStartTimestamp = ssEventData.StartTimestamp,
                    MatchPeriodStartTimestamp = ssEventData.Time.CurrentPeriodStartTimestamp,
                    InjuryTime1 = ssEventData.Time.InjuryTime1,
                    InjuryTime2 = ssEventData.Time.InjuryTime2,

                }
            };

        }

        private EventStatistics ProcessEventStatistics(SSEventStatistics ssStatistics)
        {
            EventStatistics eventStatistics = new EventStatistics();

            //Loop through all statistics, first through game periods (ALL, 1st, 2nd...)
            for (int periodIdx = 0; periodIdx < ssStatistics.Statistics.Count; periodIdx++)
            {
                var ssPeriodStatistics = ssStatistics.Statistics[periodIdx];

                Dictionary<string, Dictionary<string, StatisticsItem>> periodStats = null;

                if (eventStatistics.Statistics.ContainsKey(ssPeriodStatistics.Period))
                {
                    periodStats = eventStatistics.Statistics[ssPeriodStatistics.Period];
                }
                else
                {
                    periodStats = new();
                    eventStatistics.Statistics.Add(ssPeriodStatistics.Period, periodStats);
                }

                ProcessPeriodGroupStats(periodStats, ssPeriodStatistics);

            }

            return eventStatistics;
        }

        private void ProcessPeriodGroupStats(Dictionary<string, Dictionary<string, StatisticsItem>> periodStats, SSPeriodStatistic ssPeriodStatistics)
        {
            for (int groupIdx = 0; groupIdx < ssPeriodStatistics.Groups.Count; groupIdx++)
            {
                var ssGroupStatistics = ssPeriodStatistics.Groups[groupIdx];

                Dictionary<string, StatisticsItem> periodGroupstat = null;

                if (periodStats.ContainsKey(ssGroupStatistics.GroupName))
                {
                    periodGroupstat = periodStats[ssGroupStatistics.GroupName];
                }
                else
                {
                    periodGroupstat = new();
                    periodStats.Add(ssGroupStatistics.GroupName, periodGroupstat);
                }

                ProcessGameStats(periodGroupstat, ssGroupStatistics);
            }
        }

        private void ProcessGameStats(Dictionary<string, StatisticsItem> eventStats, SSGroupStatistic ssGroupStatistics)
        {
            for (int statIdx = 0; statIdx < ssGroupStatistics.StatisticsItems.Count; statIdx++)
            {
                var ssStat = ssGroupStatistics.StatisticsItems[statIdx];


                if (!eventStats.ContainsKey(ssStat.Key))
                {
                    var statItem = new StatisticsItem(
                        ssStat.Key, ssStat.Name,
                        ssStat.Home, ssStat.HomeValue, ssStat.HomeTotal,
                        ssStat.Away, ssStat.AwayValue, ssStat.AwayTotal,
                        ssStat.CompareCode
                        );

                    eventStats.Add(ssStat.Key, statItem);
                }
            }
        }
    }
}
