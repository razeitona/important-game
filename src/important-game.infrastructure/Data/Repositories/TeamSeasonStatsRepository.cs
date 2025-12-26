using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Dapper-based repository for TeamSeasonStats entities.
    /// Manages team statistics snapshots from Gemini API.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TeamSeasonStatsRepository : ITeamSeasonStatsRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TeamSeasonStatsRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task SaveAsync(TeamSeasonStats stats)
        {
            ArgumentNullException.ThrowIfNull(stats);

            using (var connection = _connectionFactory.CreateConnection())
            {
                var exists = await connection.ExecuteScalarAsync<int>(
                    TeamSeasonStatsQueries.CheckTeamSeasonStatsExists,
                    new { stats.TeamId, stats.CompetitionId }) > 0;

                if (exists)
                {
                    await connection.ExecuteAsync(TeamSeasonStatsQueries.UpdateTeamSeasonStats, new
                    {
                        stats.TeamId,
                        stats.CompetitionId,
                        stats.GoalsFor5,
                        stats.GoalsAgainst5,
                        stats.Wins5,
                        stats.Draws5,
                        stats.Losses5,
                        stats.UpdatedAt
                    });
                }
                else
                {
                    await connection.ExecuteAsync(TeamSeasonStatsQueries.InsertTeamSeasonStats, new
                    {
                        stats.TeamId,
                        stats.CompetitionId,
                        stats.GoalsFor5,
                        stats.GoalsAgainst5,
                        stats.Wins5,
                        stats.Draws5,
                        stats.Losses5,
                        stats.UpdatedAt
                    });
                }
            }
        }

        public async Task SaveAllAsync(List<TeamSeasonStats> statsList)
        {
            if (statsList == null || statsList.Count == 0)
                return;

            foreach (var stats in statsList)
            {
                await SaveAsync(stats);
            }
        }

        public async Task<TeamSeasonStats?> GetByTeamAndCompetitionAsync(int teamId, int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<TeamSeasonStats>(
                    TeamSeasonStatsQueries.SelectTeamSeasonStatsByTeamAndCompetition,
                    new { TeamId = teamId, CompetitionId = competitionId });
            }
        }

        public async Task<List<TeamSeasonStats>> GetByCompetitionAsync(int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<TeamSeasonStats>(
                    TeamSeasonStatsQueries.SelectTeamSeasonStatsByCompetition,
                    new { CompetitionId = competitionId });
                return result.ToList();
            }
        }

        public async Task<List<TeamSeasonStats>> GetOlderThanAsync(DateTime threshold)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<TeamSeasonStats>(
                    TeamSeasonStatsQueries.SelectTeamSeasonStatsOlderThan,
                    new { Threshold = threshold });
                return result.ToList();
            }
        }

        public async Task DeleteByTeamAndCompetitionAsync(int teamId, int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync(
                    TeamSeasonStatsQueries.DeleteTeamSeasonStatsByTeamAndCompetition,
                    new { TeamId = teamId, CompetitionId = competitionId });
            }
        }

        public async Task DeleteByCompetitionAsync(int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync(
                    TeamSeasonStatsQueries.DeleteTeamSeasonStatsByCompetition,
                    new { CompetitionId = competitionId });
            }
        }
    }
}
