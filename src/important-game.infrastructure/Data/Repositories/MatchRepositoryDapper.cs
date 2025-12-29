using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Dapper-based repository for Match entities with full CRUD and query operations.
    /// Handles complex queries involving matches, teams, competitions, and related data.
    /// </summary>
    [ExcludeFromCodeCoverage]

    [Obsolete("Class to be removed")]
    public class MatchRepositoryDapper : IMatchRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MatchRepositoryDapper(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task SaveMatchAsync(Match match)
        {
            ArgumentNullException.ThrowIfNull(match);

            using (var connection = _connectionFactory.CreateConnection())
            {
                var exists = await connection.ExecuteScalarAsync<int>(MatchQueries.CheckMatchExists, new { match.Id }) > 0;

                if (exists)
                {
                    await connection.ExecuteAsync(MatchQueries.UpdateMatch, new
                    {
                        match.Id,
                        match.CompetitionId,
                        match.MatchDateUTC,
                        match.HomeTeamId,
                        match.AwayTeamId,
                        match.HomeTeamPosition,
                        match.AwayTeamPosition,
                        match.HomeScore,
                        match.AwayScore,
                        match.HomeForm,
                        match.AwayForm,
                        MatchStatus = (int)match.MatchStatus,
                        match.ExcitmentScore,
                        match.CompetitionScore,
                        match.FixtureScore,
                        match.FormScore,
                        match.GoalsScore,
                        match.CompetitionStandingScore,
                        match.HeadToHeadScore,
                        match.RivalryScore,
                        match.TitleHolderScore,
                        match.UpdatedDateUTC,
                        match.GeminiStatsUpdatedAt
                    });
                }
                else
                {
                    await connection.ExecuteAsync(MatchQueries.InsertMatch, new
                    {
                        match.Id,
                        match.CompetitionId,
                        match.MatchDateUTC,
                        match.HomeTeamId,
                        match.AwayTeamId,
                        match.HomeTeamPosition,
                        match.AwayTeamPosition,
                        match.HomeScore,
                        match.AwayScore,
                        match.HomeForm,
                        match.AwayForm,
                        MatchStatus = (int)match.MatchStatus,
                        match.ExcitmentScore,
                        match.CompetitionScore,
                        match.FixtureScore,
                        match.FormScore,
                        match.GoalsScore,
                        match.CompetitionStandingScore,
                        match.HeadToHeadScore,
                        match.RivalryScore,
                        match.TitleHolderScore,
                        match.UpdatedDateUTC,
                        match.GeminiStatsUpdatedAt
                    });
                }
            }
        }

        public async Task<Match?> GetMatchByIdAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Match>(MatchQueries.SelectMatchById, new { Id = id });
            }
        }

        public async Task<List<Match>> GetUpcomingMatchesAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<Match>(MatchQueries.SelectUpcomingMatches, new { FinishedStatus = (int)MatchStatus.Finished });
                return result.ToList();
            }
        }

        public async Task<List<Match>> GetCompetitionActiveMatchesAsync(int competitionId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<Match>(MatchQueries.SelectCompetitionActiveMatches, new { CompetitionId = competitionId, FinishedStatus = (int)MatchStatus.Finished });
                return result.ToList();
            }
        }

        public async Task<List<Match>> GetUnfinishedMatchesAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<Match>(MatchQueries.SelectUnfinishedMatches, new { FinishedStatus = (int)MatchStatus.Finished });
                return result.ToList();
            }
        }
    }
}
