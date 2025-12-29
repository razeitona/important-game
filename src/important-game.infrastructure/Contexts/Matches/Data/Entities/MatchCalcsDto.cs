using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Matches.Data.Entities;

[ExcludeFromCodeCoverage]
public class MatchCalcsDto
{
    public int MatchId { get; set; }
    public string? HomeForm { get; set; }
    public string? AwayForm { get; set; }
    public double? ExcitmentScore { get; set; }
    public double? CompetitionScore { get; set; }
    public double? FixtureScore { get; set; }
    public double? FormScore { get; set; }
    public double? GoalsScore { get; set; }
    public double? CompetitionStandingScore { get; set; }
    public double? HeadToHeadScore { get; set; }
    public double? RivalryScore { get; set; }
    public double? TitleHolderScore { get; set; }
    public DateTimeOffset UpdatedDateUTC { get; set; } = DateTimeOffset.Now;
}
