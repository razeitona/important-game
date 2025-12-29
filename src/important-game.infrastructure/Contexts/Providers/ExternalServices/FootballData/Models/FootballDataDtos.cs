using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData.Models;

public class FootballDataCompetition
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string? Emblem { get; set; }
    public FootballDataSeason? CurrentSeason { get; set; }
    public int? NumberOfAvailableSeasons { get; set; }
}

public class FootballDataSeason
{
    public int Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CurrentMatchday { get; set; }
    public FootballDataTeamSummary? Winner { get; set; }
}

public class FootballDataTeamSummary
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class FootballDataStandingsResponse
{
    public FootballDataCompetition? Competition { get; set; }
    public FootballDataSeason? Season { get; set; }
    public List<FootballDataStandingGroup> Standings { get; set; } = new();
}

public class FootballDataStandingGroup
{
    public string? Stage { get; set; }
    public string? Type { get; set; }
    public string? Group { get; set; }
    public List<FootballDataStandingRow> Table { get; set; } = new();
}

public class FootballDataStandingRow
{
    public int Position { get; set; }
    public FootballDataTeam Team { get; set; } = new();
    public int PlayedGames { get; set; }
    public int Won { get; set; }
    public int Draw { get; set; }
    public int Lost { get; set; }
    public int Points { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference { get; set; }
}

public class FootballDataMatchesResponse
{
    public List<FootballDataMatch> Matches { get; set; } = new();
}

public class FootballDataMatch
{
    public int Id { get; set; }
    public FootballDataCompetition? Competition { get; set; }
    public FootballDataSeason? Season { get; set; }
    public DateTimeOffset UtcDate { get; set; }
    public string? Status { get; set; }
    public int? Matchday { get; set; }
    public FootballDataTeam HomeTeam { get; set; } = new();
    public FootballDataTeam AwayTeam { get; set; } = new();
    public FootballDataScore Score { get; set; } = new();
}

public class FootballDataTeam
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public string? Tla { get; set; }
}

public class FootballDataScore
{
    public FootballDataScoreResult FullTime { get; set; } = new();
    public FootballDataScoreResult HalfTime { get; set; } = new();
}

public class FootballDataScoreResult
{
    public int? Home { get; set; }
    public int? Away { get; set; }
}

public class FootballDataHeadToHeadResponse
{
    public FootballDataHeadToHeadSummary? Head2Head { get; set; }
    public List<FootballDataMatch> Matches { get; set; } = new();
}

public class FootballDataHeadToHeadSummary
{
    public FootballDataHeadToHeadTeamSummary HomeTeam { get; set; } = new();
    public FootballDataHeadToHeadTeamSummary AwayTeam { get; set; } = new();
}

public class FootballDataHeadToHeadTeamSummary
{
    [JsonPropertyName("wins")]
    public int Wins { get; set; }

    [JsonPropertyName("draws")]
    public int Draws { get; set; }

    [JsonPropertyName("losses")]
    public int Losses { get; set; }
}