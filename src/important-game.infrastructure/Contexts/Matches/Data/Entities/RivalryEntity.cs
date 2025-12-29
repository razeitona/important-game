using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Matches.Data.Entities;

[ExcludeFromCodeCoverage]
public class RivalryEntity
{
    public int TeamOneId { get; set; }
    public int TeamTwoId { get; set; }
    public double RivarlyValue { get; set; }
}
