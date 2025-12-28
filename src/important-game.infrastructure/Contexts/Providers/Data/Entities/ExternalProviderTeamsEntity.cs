using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalProviderTeamsEntity
{
    public int ProviderId { get; set; }
    public int InternalTeamId { get; set; }
    public required string ExternalTeamId { get; set; }
}

