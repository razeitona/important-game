using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalProviderCompetitionsEntity
{
    public int ProviderId { get; set; }
    public int InternalCompetitionId { get; set; }
    public required string ExternalCompetitionId { get; set; }
}
