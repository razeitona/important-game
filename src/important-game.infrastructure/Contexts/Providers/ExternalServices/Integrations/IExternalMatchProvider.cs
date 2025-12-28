using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations;
public interface IExternalMatchProvider : IExternalIntegrationProvider
{
    Task<List<ExternalMatchDto>> GetTeamFinishedMatchesAsync(int teamId, DateTimeOffset fromDate, DateTimeOffset toDate, int numberOfItems, int skipItems, CancellationToken cancellationToken = default);
}
