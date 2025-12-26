using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Integrations.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalIntegrationEntity
{
    public int IntegrationId { get; set; }
    public string Name { get; set; }
}
