using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations;
public interface IExternalMatchProvider : IExternalIntegrationProvider
{
    Task<List<ExternalMatchDto>> GetTeamFinishedMatchesAsync(string teamId, DateTimeOffset fromDate, DateTimeOffset toDate, int numberOfItems, CancellationToken cancellationToken = default);
    Task<List<ExternalMatchDto>> GetCompetitionUpcomingMatchesAsync(string competitionId, DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken = default);
}
