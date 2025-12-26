using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Dapper-based repository for Rivalry entities.
    /// Handles rivalry relationships between teams.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RivalryRepository : IRivalryRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RivalryRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task SaveRivalryAsync(Rivalry rivalry)
        {
            ArgumentNullException.ThrowIfNull(rivalry);

            using (var connection = _connectionFactory.CreateConnection())
            {
                var exists = await connection.ExecuteScalarAsync<int>(RivalryQueries.CheckRivalryExists, new { rivalry.Id }) > 0;

                if (exists)
                {
                    await connection.ExecuteAsync(RivalryQueries.UpdateRivalry, new
                    {
                        rivalry.Id,
                        rivalry.RivarlyValue
                    });
                }
                else
                {
                    await connection.ExecuteAsync(RivalryQueries.InsertRivalry, new
                    {
                        rivalry.TeamOneId,
                        rivalry.TeamTwoId,
                        rivalry.RivarlyValue
                    });
                }
            }
        }

        public async Task<Rivalry?> GetRivalryByTeamIdAsync(int teamOneId, int teamTwoId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Rivalry>(RivalryQueries.SelectRivalryByTeamId, new { TeamOneId = teamOneId, TeamTwoId = teamTwoId });
            }
        }
    }
}
