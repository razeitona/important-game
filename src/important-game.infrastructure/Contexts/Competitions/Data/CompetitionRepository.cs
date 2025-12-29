using Dapper;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Competitions.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Competitions.Data;

/// <summary>
/// Dapper-based repository for Competition entities.
/// Separates data access concerns using the Repository pattern with Dapper ORM.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompetitionRepository : ICompetitionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CompetitionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    #region Competitions
    public async Task SaveCompetitionAsync(CompetitionEntity competition)
    {
        ArgumentNullException.ThrowIfNull(competition);

        using (var connection = _connectionFactory.CreateConnection())
        {
            var exists = await connection.ExecuteScalarAsync<int>(CompetitionQueries.CheckCompetitionExists, new { competition.CompetitionId }) > 0;

            if (exists)
            {
                await connection.ExecuteAsync(CompetitionQueries.UpdateCompetition, new
                {
                    competition.CompetitionId,
                    competition.Name,
                    competition.TitleHolderTeamId
                });
            }
            else
            {
                await connection.ExecuteAsync(CompetitionQueries.InsertCompetition, new
                {
                    competition.CompetitionId,
                    competition.Name,
                    competition.PrimaryColor,
                    competition.BackgroundColor,
                    competition.LeagueRanking,
                    competition.IsActive,
                    competition.TitleHolderTeamId
                });
            }
        }
    }

    public async Task<CompetitionEntity?> GetCompetitionByIdAsync(int competitionId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<CompetitionEntity>(CompetitionQueries.SelectCompetitionById, new { CompetitionId = competitionId });
        }
    }

    public async Task<List<CompetitionEntity>> GetCompetitionsAsync()
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<CompetitionEntity>(CompetitionQueries.SelectAllCompetitions);
            return result.ToList();
        }
    }

    public async Task<List<CompetitionEntity>> GetActiveCompetitionsAsync()
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<CompetitionEntity>(CompetitionQueries.SelectActiveCompetitions);
            return result.ToList();
        }
    }
    #endregion

    #region Competition Seasons
    public async Task<CompetitionSeasonsEntity> SaveCompetitionSeasonAsync(CompetitionSeasonsEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using (var connection = _connectionFactory.CreateConnection())
        {
            var exists = await connection.ExecuteScalarAsync<int>(
                CompetitionSeasonsQueries.CheckCompetitionSeasonExists,
                new { entity.CompetitionId, entity.SeasonYear }) > 0;

            if (exists)
            {
                var existingSeason = await connection.QueryFirstOrDefaultAsync<CompetitionSeasonsEntity>(
                    CompetitionSeasonsQueries.SelectCompetitionSeasonByCompetitionAndYear,
                    new { entity.CompetitionId, entity.SeasonYear });
                return existingSeason;
            }
            else
            {
                await connection.ExecuteAsync(
                    CompetitionSeasonsQueries.InsertCompetitionSeason,
                    new { entity.CompetitionId, entity.SeasonYear, entity.TitleHolderId });

                var newSeason = await connection.QueryFirstOrDefaultAsync<CompetitionSeasonsEntity>(
                    CompetitionSeasonsQueries.SelectCompetitionSeasonByCompetitionAndYear,
                    new { entity.CompetitionId, entity.SeasonYear });
                return newSeason;
            }
        }
    }

    public async Task<CompetitionSeasonsEntity?> GetCompetitionSeasonByIdAsync(int seasonId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<CompetitionSeasonsEntity>(
                CompetitionSeasonsQueries.SelectCompetitionSeasonById,
                new { SeasonId = seasonId });
        }
    }

    public async Task<CompetitionSeasonsEntity?> GetCompetitionSeasonByCompetitionAndYearAsync(int competitionId, string seasonYear)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<CompetitionSeasonsEntity>(
                CompetitionSeasonsQueries.SelectCompetitionSeasonByCompetitionAndYear,
                new { CompetitionId = competitionId, SeasonYear = seasonYear });
        }
    }

    public async Task<List<CompetitionSeasonsEntity>> GetCompetitionSeasonsByCompetitionAsync(int competitionId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<CompetitionSeasonsEntity>(
                CompetitionSeasonsQueries.SelectCompetitionSeasonsByCompetition,
                new { CompetitionId = competitionId });
            return result.ToList();
        }
    }

    public async Task<CompetitionSeasonsEntity?> GetLatestCompetitionSeasonAsync(int competitionId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<CompetitionSeasonsEntity>(
                CompetitionSeasonsQueries.SelectLatestCompetitionSeason,
                new { CompetitionId = competitionId });
        }
    }

    public async Task DeleteCompetitionSeasonAsync(int seasonId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            await connection.ExecuteAsync(
                CompetitionSeasonsQueries.DeleteCompetitionSeason,
                new { SeasonId = seasonId });
        }
    }

    public async Task UpdateCompetitionSeasonStandingDateAsync(int seasonId, DateTimeOffset updateDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            CompetitionSeasonsQueries.UpdateCompetitionSeasonStandingDate,
            new { SeasonId = seasonId, SyncStandingsDate = updateDate });

    }
    #endregion

    #region Competition Seasons
    public async Task SaveCompetitionTableAsync(List<CompetitionTableEntity> table)
    {
        ArgumentNullException.ThrowIfNull(table);
        if (table.Count == 0)
            return;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(CompetitionTableQueries.UpsertCompetitionTable, table);
    }

    public async Task DeleteCompetitionTableByCompetitionAndSeasonAsync(int competitionId, int seasonId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            await connection.ExecuteAsync(
                CompetitionTableQueries.DeleteCompetitionTableByCompetitionAndSeason,
                new { CompetitionId = competitionId, SeasonId = seasonId });
        }
    }

    public async Task<List<CompetitionTableEntity>> GetCompetitionTableAsync(int competitionId, int seasonId)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<CompetitionTableEntity>(
                CompetitionTableQueries.SelectCompetitionTableByCompetitionAndSeason,
                new { CompetitionId = competitionId, SeasonId = seasonId });
            return result.ToList();
        }
    }
    #endregion

}
