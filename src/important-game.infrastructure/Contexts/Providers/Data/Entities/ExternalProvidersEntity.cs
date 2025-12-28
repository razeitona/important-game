using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.Data.Entities;

[ExcludeFromCodeCoverage]
public class ExternalProvidersEntity
{
    public int ProviderId { get; set; }
    public string? Name { get; set; }
    public int? MaxRequestsPerSecond { get; set; }
    public int? MaxRequestsPerMinute { get; set; }
    public int? MaxRequestsPerHour { get; set; }
    public int? MaxRequestsPerDay { get; set; }
    public int? MaxRequestsPerMonth { get; set; }
}

