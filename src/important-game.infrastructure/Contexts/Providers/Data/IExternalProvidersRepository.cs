using important_game.infrastructure.Contexts.Providers.Data.Entities;

namespace important_game.infrastructure.Contexts.Providers.Data;

/// <summary>
/// Repository interface for all external inegration tables.
/// Abstracts data access operations for external integration team mappings.
/// </summary>
public interface IExternalProvidersRepository
{
    #region Providers
    Task<ExternalProvidersEntity?> GetExternalProviderByIdAsync(int providerId);
    Task<List<ExternalProvidersLogsEntity>> GetExternalProviderLogsByIdAsync(int providerId);
    Task SaveExternalProviderLogAsync(ExternalProvidersLogsEntity entity);
    #endregion

    #region Team Integration
    Task SaveExternalIntegrationTeamAsync(ExternalProviderTeamsEntity entity);
    Task<ExternalProviderTeamsEntity?> GetExternalIntegrationTeamAsync(int providerId, int internalTeamId);
    Task<ExternalProviderTeamsEntity?> GetExternalIntegrationTeamByExternalIdAsync(int providerId, int externalTeamId);
    Task<List<ExternalProviderTeamsEntity>> GetExternalIntegrationTeamsByIntegrationAsync(int providerId);
    Task<List<ExternalProviderTeamsEntity>> GetExternalIntegrationTeamsByInternalTeamIdsAsync(int providerId, List<int> internalTeamIds);
    Task DeleteExternalIntegrationTeamAsync(int providerId, int internalTeamId);
    Task DeleteExternalIntegrationTeamsByIntegrationAsync(int providerId);
    #endregion

    #region Competition Integration
    Task SaveExternalIntegrationCompetitionAsync(ExternalProviderCompetitionsEntity entity);
    Task<ExternalProviderCompetitionsEntity?> GetExternalIntegrationCompetitionAsync(int providerId, int internalCompetitionId);
    Task<ExternalProviderCompetitionsEntity?> GetExternalCompetitionByExternalIdAsync(int providerId, string externalId);
    Task<List<ExternalProviderCompetitionsEntity>> GetExternalIntegrationCompetitionsByIntegrationAsync(int providerId);
    Task DeleteExternalIntegrationCompetitionAsync(int providerId, int internalCompetitionId);
    #endregion

    #region Competition Seasons Integration
    Task SaveExternalProviderCompetitionSeasonAsync(ExternalProviderCompetitionSeasonsEntity entity);
    Task<ExternalProviderCompetitionSeasonsEntity?> GetExternalProviderCompetitionSeasonAsync(int providerId, int seasonId);
    Task<ExternalProviderCompetitionSeasonsEntity?> GetExternalProviderCompetitionSeasonByExternalIdAsync(int providerId, string externalSeasonId);
    #endregion

    #region Matches Integration
    Task SaveExternalProviderMatchAsync(ExternalProviderMatchesEntity entity);
    Task<ExternalProviderMatchesEntity?> GetExternalProviderMatchAsync(int providerId, int internalMatchId);
    Task<List<ExternalProviderMatchesEntity>> GetExternalProviderMatchesByProviderAsync(int providerId);
    Task DeleteExternalProviderMatchAsync(int providerId, int internalMatchId);
    #endregion
}