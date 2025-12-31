using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Teams.Data.Entities;
using important_game.infrastructure.Contexts.Teams.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Teams.Data;

/// <summary>
/// Dapper-based repository for Team entities.
/// Separates data access concerns using the Repository pattern with Dapper ORM.
/// </summary>
[ExcludeFromCodeCoverage]
public class TeamRepository : ITeamRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TeamRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<List<TeamEntity>> GetAllTeamsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<TeamEntity>(TeamQueries.SelectAllTeams);
        return result.ToList();
    }

    public async Task UpdateTeamAsync(TeamEntity entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(TeamQueries.UpdateTeam,
            new
            {
                entity.Id,
                entity.Name,
                entity.ShortName,
                entity.ThreeLetterName,
                entity.NormalizedName,
                entity.SlugName
            }
        );
    }

    public async Task<TeamEntity> SaveTeamAsync(TeamEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using (var connection = _connectionFactory.CreateConnection())
        {
            var exists = await connection.ExecuteScalarAsync<int>(TeamQueries.CheckTeamExists, new { entity.Id }) > 0;

            if (!exists)
            {
                var insertedId = await connection.ExecuteScalarAsync<int>(TeamQueries.InsertTeam,
                    new { entity.Name, entity.ShortName, entity.ThreeLetterName, entity.NormalizedName, entity.SlugName });
                entity.Id = insertedId;
            }
        }

        return entity;
    }



    public async Task<Team> SaveTeamAsync(Team team)
    {
        ArgumentNullException.ThrowIfNull(team);

        using (var connection = _connectionFactory.CreateConnection())
        {
            var exists = await connection.ExecuteScalarAsync<int>(TeamQueries.CheckTeamExists, new { team.Id }) > 0;

            if (!exists)
            {
                await connection.ExecuteAsync(TeamQueries.InsertTeam, new { team.Id, team.Name });
            }
        }

        return team;
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        using (var connection = _connectionFactory.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<Team>(TeamQueries.SelectTeamById, new { Id = id });
        }
    }

    public async Task<List<Team>> GetTeamsByIdsAsync(List<int> ids)
    {
        if (ids == null || ids.Count == 0)
            return new List<Team>();

        using (var connection = _connectionFactory.CreateConnection())
        {
            var result = await connection.QueryAsync<Team>(TeamQueries.SelectTeamsByIds, new { Ids = ids });
            return result.ToList();
        }
    }
}
