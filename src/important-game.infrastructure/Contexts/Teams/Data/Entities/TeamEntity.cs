using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Teams.Data.Entities;

[ExcludeFromCodeCoverage]
public class TeamEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ShortName { get; set; }
    public string? ThreeLetterName { get; set; }
    public string NormalizedName { get; set; }
}

