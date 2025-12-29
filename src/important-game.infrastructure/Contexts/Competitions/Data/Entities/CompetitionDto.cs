using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Competitions.Data.Entities;

[ExcludeFromCodeCoverage]
public class CompetitionDto
{
    public int CompetitionId { get; set; }
    public required string Name { get; set; }
    public required string PrimaryColor { get; set; }
    public required string BackgroundColor { get; set; }
}
