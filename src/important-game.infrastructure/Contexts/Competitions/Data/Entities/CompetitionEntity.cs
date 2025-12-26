using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Competitions.Data.Entities;

[ExcludeFromCodeCoverage]
public class CompetitionEntity
{
    public int CompetitionId { get; set; }
    public required string Name { get; set; }
    public required string PrimaryColor { get; set; }
    public required string BackgroundColor { get; set; }
    public double LeagueRanking { get; set; }
    public bool IsActive { get; set; }
    public int? TitleHolderTeamId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
