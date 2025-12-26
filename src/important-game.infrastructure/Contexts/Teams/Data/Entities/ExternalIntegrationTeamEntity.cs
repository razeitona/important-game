using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Teams.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalIntegrationTeamEntity
{
    public int IntegrationId { get; set; }
    public int InternalTeamId { get; set; }
    public int ExternalTeamId { get; set; }
}

