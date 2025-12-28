using FuzzySharp;
using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;
using important_game.infrastructure.Contexts.Teams.Data;
using important_game.infrastructure.Contexts.Teams.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices;

public interface IExternalMatchesSyncService
{
    Task SyncMatchesAsync(CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public class ExternalMatchesSyncService : IExternalMatchesSyncService
{
    private const string CurrentSeasonYear = "2025/2026";

    private readonly IIntegrationProviderFactory _integrationProvider;
    private readonly IExternalProvidersRepository _externalIntegrationRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<ExternalMatchesSyncService> _logger;

    public ExternalMatchesSyncService(
        IIntegrationProviderFactory integrationProvider,
        IExternalProvidersRepository externalIntegrationRepository,
        ITeamRepository teamRepository,
        ICompetitionRepository competitionRepository,
        ILogger<ExternalMatchesSyncService> logger)
    {
        _integrationProvider = integrationProvider ?? throw new ArgumentNullException(nameof(integrationProvider));
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _externalIntegrationRepository = externalIntegrationRepository ?? throw new ArgumentNullException(nameof(externalIntegrationRepository));
        _competitionRepository = competitionRepository ?? throw new ArgumentNullException(nameof(competitionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SyncMatchesAsync(CancellationToken cancellationToken = default)
    {
       
    }

}
