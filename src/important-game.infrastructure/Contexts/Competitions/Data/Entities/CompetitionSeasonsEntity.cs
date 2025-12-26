using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Competitions.Data.Entities;

[ExcludeFromCodeCoverage]
public class CompetitionSeasonsEntity
{
    public int SeasonId { get; set; }
    public int CompetitionId { get; set; }
    public required string SeasonYear { get; set; }
    public int TitleHolderId { get; set; }
}
