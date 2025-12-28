using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalProviderCompetitionSeasonsEntity
{
    public int ProviderId { get; set; }
    public int InternalSeasonId { get; set; }
    public required string ExternalSeasonId { get; set; }
}

