using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;

namespace important_game.infrastructure.Data.Repositories
{
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

        public async Task<CompetitionEntity?> GetCompetitionByIdAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CompetitionEntity>(CompetitionQueries.SelectCompetitionById, new { Id = id });
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
    }
}
