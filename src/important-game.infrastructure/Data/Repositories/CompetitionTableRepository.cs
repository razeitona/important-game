using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Dapper-based repository for CompetitionTable entities.
    /// Manages competition standings snapshots from Gemini API.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CompetitionTableRepository : ICompetitionTableRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CompetitionTableRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task SaveAsync(CompetitionTable table)
        {
            ArgumentNullException.ThrowIfNull(table);

            using (var connection = _connectionFactory.CreateConnection())
            {
                var exists = await connection.ExecuteScalarAsync<int>(
                    CompetitionTableQueries.CheckCompetitionTableExists,
                    new { table.CompetitionId, table.TeamId }) > 0;

                if (exists)
                {
                    await connection.ExecuteAsync(CompetitionTableQueries.UpdateCompetitionTable, new
                    {
                        table.CompetitionId,
                        table.TeamId,
                        table.Position,
                        table.Points,
                        table.Matches,
                        table.Wins,
                        table.Draws,
                        table.Losses,
                        table.GoalsFor,
                        table.GoalsAgainst,
                        table.UpdatedAt
                    });
                }
                else
                {
                    await connection.ExecuteAsync(CompetitionTableQueries.InsertCompetitionTable, new
                    {
                        table.CompetitionId,
                        table.TeamId,
                        table.Position,
                        table.Points,
                        table.Matches,
                        table.Wins,
                        table.Draws,
                        table.Losses,
                        table.GoalsFor,
                        table.GoalsAgainst,
                        table.UpdatedAt
                    });
                }
            }
        }

        public async Task SaveAllAsync(List<CompetitionTable> tables)
        {
            if (tables == null || tables.Count == 0)
                return;

            using (var connection = _connectionFactory.CreateConnection())
            {
                var competitionId = tables[0].CompetitionId;

                // Delete existing entries for this competition
                await connection.ExecuteAsync(
                    CompetitionTableQueries.DeleteCompetitionTableByCompetitionId,
                    new { CompetitionId = competitionId });

                // Insert new entries
                foreach (var table in tables)
                {
                    await connection.ExecuteAsync(CompetitionTableQueries.InsertCompetitionTable, new
                    {
                        table.CompetitionId,
                        table.TeamId,
                        table.Position,
                        table.Points,
                        table.Matches,
                        table.Wins,
                        table.Draws,
                        table.Losses,
                        table.GoalsFor,
                        table.GoalsAgainst,
                        table.UpdatedAt
                    });
                }
            }
        }

        public async Task<List<CompetitionTable>> GetByCompetitionIdAsync(int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<CompetitionTable>(
                    CompetitionTableQueries.SelectCompetitionTableByCompetitionId,
                    new { CompetitionId = competitionId });
                return result.ToList();
            }
        }

        public async Task<CompetitionTable?> GetByTeamAndCompetitionAsync(int teamId, int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CompetitionTable>(
                    CompetitionTableQueries.SelectCompetitionTableByTeamAndCompetition,
                    new { CompetitionId = competitionId, TeamId = teamId });
            }
        }

        public async Task<DateTime?> GetLastUpdateAsync(int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<DateTime?>(
                    CompetitionTableQueries.SelectLastCompetitionTableUpdate,
                    new { CompetitionId = competitionId });
                return result;
            }
        }

        public async Task DeleteByCompetitionIdAsync(int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync(
                    CompetitionTableQueries.DeleteCompetitionTableByCompetitionId,
                    new { CompetitionId = competitionId });
            }
        }
    }
}
