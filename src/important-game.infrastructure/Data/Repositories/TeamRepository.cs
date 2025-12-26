using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;

namespace important_game.infrastructure.Data.Repositories
{
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
}
