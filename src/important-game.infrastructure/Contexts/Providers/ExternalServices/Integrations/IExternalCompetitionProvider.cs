using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations;
public interface IExternalCompetitionProvider : IExternalIntegrationProvider
{
    Task<ExternalCompetitionDto?> GetCompetitionAsync(string competitionId, CancellationToken cancellationToken = default);
    Task<ExternalCompetitionStandingsDto?> GetCompetitionStandingsAsync(string competitionId, string seasonId, CancellationToken cancellationToken = default);
}