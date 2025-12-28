using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalProvidersLogsEntity
{
    public int ProviderId { get; set; }
    public required string RequestPath { get; set; }
    public DateTimeOffset RequestDate { get; set; }
}

