using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Matches.Data.Entities;

[ExcludeFromCodeCoverage]
public class MatchesEntity
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public int SeasonId { get; set; }
    public DateTimeOffset MatchDateUTC { get; set; }
    public int HomeTeamId { get; set; }
    public int? HomeTeamPosition { get; set; }
    public int AwayTeamId { get; set; }
    public int? AwayTeamPosition { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string? HomeForm { get; set; }
    public string? AwayForm { get; set; }
    public bool IsFinished { get; set; }
    public double? ExcitmentScore { get; set; }
    public double? CompetitionScore { get; set; }
    public double? FixtureScore { get; set; }
    public double? FormScore { get; set; }
    public double? GoalsScore { get; set; }
    public double? CompetitionStandingScore { get; set; }
    public double? HeadToHeadScore { get; set; }
    public double? RivalryScore { get; set; }
    public double? TitleHolderScore { get; set; }
    public DateTimeOffset UpdatedDateUTC { get; set; }
}
