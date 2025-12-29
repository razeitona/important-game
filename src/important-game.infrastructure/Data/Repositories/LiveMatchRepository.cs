using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories.Queries;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Dapper-based repository for LiveMatch entities.
    /// Handles snapshots of live match data with various statistics.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LiveMatchRepository : ILiveMatchRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public LiveMatchRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task SaveLiveMatchAsync(LiveMatch liveMatch)
        {
            ArgumentNullException.ThrowIfNull(liveMatch);

            using (var connection = _connectionFactory.CreateConnection())
            {
                var exists = await connection.ExecuteScalarAsync<int>(LiveMatchQueries.CheckLiveMatchExists, new { liveMatch.Id }) > 0;

                if (exists)
                {
                    await connection.ExecuteAsync(LiveMatchQueries.UpdateLiveMatch, new
                    {
                        liveMatch.Id,
                        liveMatch.ExcitmentScore,
                        liveMatch.ScoreLineScore,
                        liveMatch.ShotTargetScore,
                        liveMatch.XGoalsScore,
                        liveMatch.TotalFoulsScore,
                        liveMatch.TotalCardsScore,
                        liveMatch.PossesionScore,
                        liveMatch.BigChancesScore
                    });
                }
                else
                {
                    await connection.ExecuteAsync(LiveMatchQueries.InsertLiveMatch, new
                    {
                        liveMatch.MatchId,
                        liveMatch.RegisteredDate,
                        liveMatch.HomeScore,
                        liveMatch.AwayScore,
                        liveMatch.Minutes,
                        liveMatch.ExcitmentScore,
                        liveMatch.ScoreLineScore,
                        liveMatch.ShotTargetScore,
                        liveMatch.XGoalsScore,
                        liveMatch.TotalFoulsScore,
                        liveMatch.TotalCardsScore,
                        liveMatch.PossesionScore,
                        liveMatch.BigChancesScore
                    });
                }
            }
        }
    }
}
