using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalProviderMatchesEntity
{
    public int ProviderId { get; set; }
    public int InternalMatchId { get; set; }
    public string ExternalMatchId { get; set; } = string.Empty;
}
