using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using important_game.infrastructure.FootballData;
using important_game.infrastructure.FootballData.Models;
using important_game.infrastructure.ImportantMatch.Models;
using important_game.infrastructure.ImportantMatch.Models.Processors;

namespace important_game.infrastructure.LeagueProcessors;

[ExcludeFromCodeCoverage]
internal class FootballDataLeagueProcessor(IFootballDataIntegration integration) : ILeagueProcessor
{
    private readonly IFootballDataIntegration _integration = integration ?? throw new ArgumentNullException(nameof(integration));

    public async Task<League> GetLeagueDataAsync(string leagueId)
    {
        if (string.IsNullOrWhiteSpace(leagueId))
        {
            return null;
        }

        var competition = await _integration.GetCompetitionAsync(leagueId).ConfigureAwait(false);
        if (competition?.CurrentSeason == null)
        {
            return null;
        }

        var season = competition.CurrentSeason;
        var seasonName = BuildSeasonName(season);

        return new League
        {
            Id = competition.Id,
            Name = competition.Name ?? string.Empty,
            CurrentSeason = new LeagueSeason
            {
                Id = season.Id,
                Name = seasonName,
                Round = season.CurrentMatchday ?? 0
            },
            TitleHolder = CreateTitleHolder(season.Winner),
            Ranking = competition.NumberOfAvailableSeasons ?? 0d
        };
    }

    public async Task<LeagueUpcomingFixtures> GetUpcomingMatchesAsync(int leagueId, int seasonId)
    {
        var upcomingFixtures = new LeagueUpcomingFixtures();
        var matches = await _integration.GetUpcomingMatchesAsync(leagueId, daysAhead: 7).ConfigureAwait(false);

        if (matches.Count == 0)
        {
            return upcomingFixtures;
        }

        var now = DateTimeOffset.UtcNow;
        var teamFormCache = new Dictionary<int, Task<TeamFixtureData>>();

        foreach (var match in matches.OrderBy(m => m.UtcDate))
        {
            if (match.HomeTeam == null || match.AwayTeam == null)
            {
                continue;
            }

            var matchStart = match.UtcDate;
            if (matchStart > now.AddDays(5) || matchStart < now.AddHours(-3))
            {
                continue;
            }

            var homeFixturesTask = GetTeamFixturesAsync(match.HomeTeam.Id, teamFormCache);
            var awayFixturesTask = GetTeamFixturesAsync(match.AwayTeam.Id, teamFormCache);
            var headToHeadTask = GetHeadToHeadAsync(match.Id);

            await Task.WhenAll(homeFixturesTask, awayFixturesTask, headToHeadTask).ConfigureAwait(false);

            upcomingFixtures.Add(new UpcomingFixture
            {
                Id = match.Id,
                MatchDate = matchStart,
                HomeTeam = new TeamInfo
                {
                    Id = match.HomeTeam.Id,
                    Name = match.HomeTeam.Name ?? string.Empty,
                    LastFixtures = homeFixturesTask.Result
                },
                AwayTeam = new TeamInfo
                {
                    Id = match.AwayTeam.Id,
                    Name = match.AwayTeam.Name ?? string.Empty,
                    LastFixtures = awayFixturesTask.Result
                },
                HeadToHead = headToHeadTask.Result
            });
        }

        return upcomingFixtures;
    }

    public async Task<LeagueStanding> GetLeagueTableAsync(int leagueId, int seasonId)
    {
        var standingsResponse = await _integration.GetCompetitionStandingsAsync(leagueId).ConfigureAwait(false);
        if (standingsResponse?.Standings == null || standingsResponse.Standings.Count == 0)
        {
            return null;
        }

        var totalStanding = standingsResponse.Standings
            .FirstOrDefault(s => string.Equals(s.Type, "TOTAL", StringComparison.OrdinalIgnoreCase))
            ?? standingsResponse.Standings.FirstOrDefault();

        if (totalStanding == null || totalStanding.Table == null || totalStanding.Table.Count == 0)
        {
            return null;
        }

        var leagueStanding = new LeagueStanding
        {
            LeagueId = leagueId,
            CurrentRound = standingsResponse.Season?.CurrentMatchday ?? 0,
            TotalRounds = DetermineTotalRounds(totalStanding.Table.Count),
            Standings = new List<Standing>()
        };

        foreach (var row in totalStanding.Table)
        {
            leagueStanding.Standings.Add(new Standing
            {
                Team = new TeamInfo
                {
                    Id = row.Team?.Id ?? 0,
                    Name = row.Team?.Name ?? string.Empty
                },
                Position = row.Position,
                Matches = row.PlayedGames,
                Wins = row.Won,
                Losses = row.Lost,
                Draws = row.Draw,
                Points = row.Points,
                GoalsFor = row.GoalsFor,
                GoalsAgainst = row.GoalsAgainst
            });
        }

        if (leagueStanding.CurrentRound > leagueStanding.TotalRounds)
        {
            leagueStanding.TotalRounds = leagueStanding.CurrentRound;
        }

        return leagueStanding;
    }

    public Task<EventStatistics?> GetEventStatisticsAsync(string eventId)
    {
        // The public Football-Data API does not expose shot-level statistics in the free tier.
        return Task.FromResult<EventStatistics?>(null);
    }

    public async Task<EventInfo> GetEventInformationAsync(string eventId)
    {
        if (!int.TryParse(eventId, out var matchId))
        {
            return null;
        }

        var match = await _integration.GetMatchAsync(matchId).ConfigureAwait(false);
        if (match == null || match.HomeTeam == null || match.AwayTeam == null)
        {
            return null;
        }

        var fullTime = match.Score?.FullTime;
        var homeScore = fullTime?.Home ?? 0;
        var awayScore = fullTime?.Away ?? 0;

        return new EventInfo
        {
            Id = match.Id,
            MatchDate = match.UtcDate,
            HomeTeam = new TeamInfo
            {
                Id = match.HomeTeam.Id,
                Name = match.HomeTeam.Name ?? string.Empty
            },
            AwayTeam = new TeamInfo
            {
                Id = match.AwayTeam.Id,
                Name = match.AwayTeam.Name ?? string.Empty
            },
            HomeTeamScore = homeScore,
            AwayTeamScore = awayScore,
            Status = new EventStatus
            {
                Status = match.Status ?? string.Empty,
                StatusCode = MapStatus(match.Status),
                Period = null,
                MatchStartTimestamp = match.UtcDate.ToUnixTimeSeconds(),
                MatchPeriodStartTimestamp = match.UtcDate.ToUnixTimeSeconds()
            }
        };
    }

    private static int DetermineTotalRounds(int teamCount)
    {
        if (teamCount <= 1)
        {
            return teamCount;
        }

        // Assume a double round-robin format when detailed scheduling information is unavailable.
        return (teamCount - 1) * 2;
    }

    private static string BuildSeasonName(FootballDataSeason season)
    {
        if (season.StartDate is null || season.EndDate is null)
        {
            return season.Id.ToString();
        }

        var startYear = season.StartDate.Value.Year;
        var endYear = season.EndDate.Value.Year;
        return startYear == endYear ? startYear.ToString() : $"{startYear}/{endYear % 100:00}";
    }

    private static TeamTitleHolder CreateTitleHolder(FootballDataTeamSummary? winner)
    {
        if (winner == null)
        {
            return new TeamTitleHolder { Id = 0, Name = string.Empty };
        }

        return new TeamTitleHolder
        {
            Id = winner.Id,
            Name = winner.Name ?? string.Empty
        };
    }

    private async Task<TeamFixtureData> GetTeamFixturesAsync(int teamId, IDictionary<int, Task<TeamFixtureData>> cache)
    {
        if (!cache.TryGetValue(teamId, out var fixtureTask))
        {
            fixtureTask = FetchTeamFixturesAsync(teamId);
            cache[teamId] = fixtureTask;
        }

        return await fixtureTask.ConfigureAwait(false);
    }

    private async Task<TeamFixtureData> FetchTeamFixturesAsync(int teamId)
    {
        var matches = await _integration.GetTeamMatchesAsync(teamId).ConfigureAwait(false);
        var orderedMatches = matches
            .Where(m => m.Score != null)
            .OrderByDescending(m => m.UtcDate)
            .Take(FootballDataConstants.DefaultRecentMatchesLimit)
            .ToList();

        TeamFixtureData data = new()
        {
            AmountOfGames = orderedMatches.Count
        };

        foreach (var match in orderedMatches)
        {
            var result = EvaluateFixtureResult(teamId, match, out var goalsFor, out var goalsAgainst);
            data.GoalsFor += goalsFor;
            data.GoalsAgainst += goalsAgainst;

            switch (result)
            {
                case MatchResultType.Win:
                    data.Wins++;
                    break;
                case MatchResultType.Draw:
                    data.Draws++;
                    break;
                case MatchResultType.Lost:
                    data.Lost++;
                    break;
            }

            data.FixtureResult.Add(result);
        }

        return data;
    }

    private static MatchResultType EvaluateFixtureResult(int teamId, FootballDataMatch match, out int goalsFor, out int goalsAgainst)
    {
        goalsFor = 0;
        goalsAgainst = 0;

        var fullTime = match.Score?.FullTime;
        if (fullTime == null)
        {
            return MatchResultType.Draw;
        }

        var isHome = match.HomeTeam?.Id == teamId;
        goalsFor = isHome ? fullTime.Home ?? 0 : fullTime.Away ?? 0;
        goalsAgainst = isHome ? fullTime.Away ?? 0 : fullTime.Home ?? 0;

        if (goalsFor > goalsAgainst)
        {
            return MatchResultType.Win;
        }

        if (goalsFor < goalsAgainst)
        {
            return MatchResultType.Lost;
        }

        return MatchResultType.Draw;
    }

    private async Task<List<Fixture>> GetHeadToHeadAsync(int matchId)
    {
        var headToHead = await _integration.GetHeadToHeadAsync(matchId).ConfigureAwait(false);
        if (headToHead?.Matches == null || headToHead.Matches.Count == 0)
        {
            return new List<Fixture>();
        }

        return headToHead.Matches
            .Where(m => m.HomeTeam != null && m.AwayTeam != null)
            .OrderByDescending(m => m.UtcDate)
            .Take(FootballDataConstants.DefaultRecentMatchesLimit)
            .Select(MapFixture)
            .ToList();
    }

    private static Fixture MapFixture(FootballDataMatch match)
    {
        var fullTime = match.Score?.FullTime;
        return new Fixture
        {
            Id = match.Id,
            MatchDate = match.UtcDate,
            HomeTeam = new TeamInfo
            {
                Id = match.HomeTeam?.Id ?? 0,
                Name = match.HomeTeam?.Name ?? string.Empty
            },
            AwayTeam = new TeamInfo
            {
                Id = match.AwayTeam?.Id ?? 0,
                Name = match.AwayTeam?.Name ?? string.Empty
            },
            HomeTeamScore = fullTime?.Home ?? 0,
            AwayTeamScore = fullTime?.Away ?? 0
        };
    }

    private static EventMatchStatus MapStatus(string? status)
    {
        return status switch
        {
            "TIMED" or "SCHEDULED" => EventMatchStatus.NotStarted,
            "IN_PLAY" or "PAUSED" => EventMatchStatus.FirstHalf,
            "FINISHED" => EventMatchStatus.Finished,
            _ => EventMatchStatus.NotStarted
        };
    }
}