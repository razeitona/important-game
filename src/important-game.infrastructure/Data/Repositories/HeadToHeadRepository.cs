using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Dapper-based repository for Head-to-Head match history.
    /// Manages historical match data between two teams.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HeadToHeadRepository : IHeadToHeadRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public HeadToHeadRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task SaveHeadToHeadMatchesAsync(List<Headtohead> headtoheadMatches)
        {
            if (headtoheadMatches == null || headtoheadMatches.Count == 0)
                return;

            using (var connection = _connectionFactory.CreateConnection())
            {
                var matchId = headtoheadMatches[0].MatchId;

                await connection.ExecuteAsync(HeadToHeadQueries.DeleteHeadToHeadByMatchId, new { MatchId = matchId });

                foreach (var match in headtoheadMatches)
                {
                    await connection.ExecuteAsync(HeadToHeadQueries.InsertHeadToHead, new
                    {
                        match.MatchId,
                        match.HomeTeamId,
                        match.AwayTeamId,
                        match.MatchDateUTC,
                        match.HomeTeamScore,
                        match.AwayTeamScore
                    });
                }
            }
        }
    }
}
