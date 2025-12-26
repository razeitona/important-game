using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Teams.Data.Entities;

[ExcludeFromCodeCoverage]
public class TeamEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}

