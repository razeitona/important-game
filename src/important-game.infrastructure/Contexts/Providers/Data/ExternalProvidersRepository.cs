using Dapper;
using important_game.infrastructure.Contexts.Providers.Data.Entities;
using important_game.infrastructure.Contexts.Providers.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.Data;

/// <summary>
/// Dapper-based repository for ExternalIntegrationTeam entities.
/// Separates data access concerns using the Repository pattern with Dapper ORM.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExternalProvidersRepository(IDbConnectionFactory connectionFactory) : IExternalProvidersRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    #region Providers
    public async Task<ExternalProvidersEntity?> GetExternalProviderByIdAsync(int providerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<ExternalProvidersEntity>(
            ExternalProvidersQueries.SelectExternalProviderById,
            new { ProviderId = providerId });
        return result;
    }

    public async Task<List<ExternalProvidersLogsEntity>> GetExternalProviderLogsByIdAsync(int providerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ExternalProvidersLogsEntity>(
            ExternalProvidersQueries.SelectExternalProviderLogsById,
            new { ProviderId = providerId });
        return result.ToList();
    }

    public async Task SaveExternalProviderLogAsync(ExternalProvidersLogsEntity entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            ExternalProvidersQueries.InsertExternalProviderLog,
            new { entity.ProviderId, entity.RequestPath, entity.RequestDate });
    }
    #endregion

    #region Team Integration
    public async Task SaveExternalIntegrationTeamAsync(ExternalProviderTeamsEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var connection = _connectionFactory.CreateConnection();
        var exists = await connection.ExecuteScalarAsync<int>(
            ExternalProviderTeamsQueries.CheckExternalProviderTeamExists,
            new { entity.ProviderId, entity.InternalTeamId }) > 0;

        if (exists)
        {
            await connection.ExecuteAsync(
                ExternalProviderTeamsQueries.UpdateExternalProviderTeam,
                new { entity.ProviderId, entity.InternalTeamId, entity.ExternalTeamId });
        }
        else
        {
            await connection.ExecuteAsync(
                ExternalProviderTeamsQueries.InsertExternalProviderTeam,
                new { entity.ProviderId, entity.InternalTeamId, entity.ExternalTeamId });
        }
    }

    public async Task<ExternalProviderTeamsEntity?> GetExternalIntegrationTeamAsync(int providerId, int internalTeamId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ExternalProviderTeamsEntity>(
            ExternalProviderTeamsQueries.SelectExternalProviderTeamByIds,
            new { ProviderId = providerId, InternalTeamId = internalTeamId });
    }

    public async Task<ExternalProviderTeamsEntity?> GetExternalIntegrationTeamByExternalIdAsync(int providerId, int externalTeamId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ExternalProviderTeamsEntity>(
            ExternalProviderTeamsQueries.SelectExternalProviderTeamByExternalId,
            new { ProviderId = providerId, ExternalTeamId = externalTeamId });
    }

    public async Task<List<ExternalProviderTeamsEntity>> GetExternalIntegrationTeamsByIntegrationAsync(int providerId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<ExternalProviderTeamsEntity>(
                ExternalProviderTeamsQueries.SelectExternalProviderTeamsByProvider,
                new { ProviderId = providerId });
            return result.ToList();
        }
    }

    public async Task<List<ExternalProviderTeamsEntity>> GetExternalIntegrationTeamsByInternalTeamIdsAsync(int providerId, List<int> internalTeamIds)
    {
        if (internalTeamIds == null || internalTeamIds.Count == 0)
            return new List<ExternalProviderTeamsEntity>();

        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<ExternalProviderTeamsEntity>(
                ExternalProviderTeamsQueries.SelectExternalProviderTeamsByInternalTeamIds,
                new { ProviderId = providerId, InternalTeamIds = internalTeamIds });
            return result.ToList();
        }
    }

    public async Task DeleteExternalIntegrationTeamAsync(int providerId, int internalTeamId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            await connection.ExecuteAsync(
                ExternalProviderTeamsQueries.DeleteExternalProviderTeam,
                new { ProviderId = providerId, InternalTeamId = internalTeamId });
        }
    }

    public async Task DeleteExternalIntegrationTeamsByIntegrationAsync(int providerId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            await connection.ExecuteAsync(
                ExternalProviderTeamsQueries.DeleteExternalProviderTeamsByProvider,
                new { ProviderId = providerId });
        }
    }

    #endregion

    #region Competition Integration

    public async Task SaveExternalIntegrationCompetitionAsync(ExternalProviderCompetitionsEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using (var connection = _connectionFactory.CreateConnection())
        {
            var exists = await connection.ExecuteScalarAsync<int>(
                ExternalProviderCompetitionsQueries.CheckExternalProviderCompetitionExists,
                new { entity.ProviderId, entity.InternalCompetitionId }) > 0;

            if (exists)
            {
                await connection.ExecuteAsync(
                    ExternalProviderCompetitionsQueries.UpdateExternalProviderCompetition,
                    new { entity.ProviderId, entity.InternalCompetitionId, entity.ExternalCompetitionId });
            }
            else
            {
                await connection.ExecuteAsync(
                    ExternalProviderCompetitionsQueries.InsertExternalProviderCompetition,
                    new { entity.ProviderId, entity.InternalCompetitionId, entity.ExternalCompetitionId });
            }
        }
    }

    public async Task<ExternalProviderCompetitionsEntity?> GetExternalIntegrationCompetitionAsync(int providerId, int internalCompetitionId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<ExternalProviderCompetitionsEntity>(
                ExternalProviderCompetitionsQueries.SelectExternalProviderCompetitionByInternalId,
                new { ProviderId = providerId, InternalCompetitionId = internalCompetitionId });
        }
    }

    public async Task<ExternalProviderCompetitionsEntity?> GetExternalCompetitionByExternalIdAsync(int providerId, string externalId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<ExternalProviderCompetitionsEntity>(
                ExternalProviderCompetitionsQueries.SelectExternalCompetitionByExternalId,
                new { ProviderId = providerId, ExternalCompetitionId = externalId });
        }
    }

    public async Task<List<ExternalProviderCompetitionsEntity>> GetExternalIntegrationCompetitionsByIntegrationAsync(int providerId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<ExternalProviderCompetitionsEntity>(
                ExternalProviderCompetitionsQueries.SelectExternalProviderCompetitionsByProvider,
                new { ProviderId = providerId });
            return result.ToList();
        }
    }

    public async Task DeleteExternalIntegrationCompetitionAsync(int providerId, int internalCompetitionId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            await connection.ExecuteAsync(
                ExternalProviderCompetitionsQueries.DeleteExternalProviderCompetition,
                new { ProviderId = providerId, InternalCompetitionId = internalCompetitionId });
        }
    }

    #endregion

    #region Competition Seasons Integration

    public async Task SaveExternalProviderCompetitionSeasonAsync(ExternalProviderCompetitionSeasonsEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using (var connection = _connectionFactory.CreateConnection())
        {
            var exists = await connection.ExecuteScalarAsync<int>(
                ExternalProviderCompetitionSeasonsQueries.CheckExternalProviderSeasonExists,
                new { entity.ProviderId, entity.InternalSeasonId }) > 0;

            if (exists)
            {
                await connection.ExecuteAsync(
                    ExternalProviderCompetitionSeasonsQueries.UpdateExternalProviderSeason,
                    new { entity.ProviderId, entity.InternalSeasonId, entity.ExternalSeasonId });
            }
            else
            {
                await connection.ExecuteAsync(
                    ExternalProviderCompetitionSeasonsQueries.InsertExternalProviderSeason,
                    new { entity.ProviderId, entity.InternalSeasonId, entity.ExternalSeasonId });
            }
        }
    }

    public async Task<ExternalProviderCompetitionSeasonsEntity?> GetExternalProviderCompetitionSeasonAsync(int providerId, int seasonId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<ExternalProviderCompetitionSeasonsEntity>(
                ExternalProviderCompetitionSeasonsQueries.SelectExternalProviderCompetitionSeasonByInternalId,
                new { ProviderId = providerId, InternalSeasonId = seasonId });
        }
    }

    public async Task<ExternalProviderCompetitionSeasonsEntity?> GetExternalProviderCompetitionSeasonByExternalIdAsync(int providerId, string externalSeasonId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<ExternalProviderCompetitionSeasonsEntity>(
                ExternalProviderCompetitionSeasonsQueries.SelectExternalProviderCompetitionSeasonByExternalId,
                new { ProviderId = providerId, ExternalSeasonId = externalSeasonId });
        }
    }

    #endregion

    #region Matches Integration

    public async Task SaveExternalProviderMatchAsync(ExternalProviderMatchesEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            ExternalProviderMatchesQueries.InsertExternalProviderMatch,
            new
            {
                entity.ProviderId,
                entity.InternalMatchId,
                entity.ExternalMatchId
            });
    }

    public async Task<ExternalProviderMatchesEntity?> GetExternalProviderMatchAsync(int providerId, int internalMatchId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<ExternalProviderMatchesEntity>(
            ExternalProviderMatchesQueries.SelectExternalProviderMatchByIds,
            new
            {
                ProviderId = providerId,
                InternalMatchId = internalMatchId
            });
    }

    public async Task<List<ExternalProviderMatchesEntity>> GetExternalProviderMatchesByProviderAsync(int providerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ExternalProviderMatchesEntity>(
            ExternalProviderMatchesQueries.SelectExternalProviderMatchesByProvider,
            new { ProviderId = providerId });
        return result.ToList();
    }

    public async Task DeleteExternalProviderMatchAsync(int providerId, int internalMatchId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(ExternalProviderMatchesQueries.DeleteExternalProviderMatch, new
        {
            ProviderId = providerId,
            InternalMatchId = internalMatchId
        });
    }
    #endregion

}
