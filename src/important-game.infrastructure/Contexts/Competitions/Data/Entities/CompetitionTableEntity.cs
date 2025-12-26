using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Competitions.Data.Entities;

[ExcludeFromCodeCoverage]
public class CompetitionTableEntity
{
    public int CompetitionId { get; set; }
    public int SeasonId { get; set; }
    public int TeamId { get; set; }
    public int Position { get; set; }
    public int Points { get; set; }
    public int Matches { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
}
