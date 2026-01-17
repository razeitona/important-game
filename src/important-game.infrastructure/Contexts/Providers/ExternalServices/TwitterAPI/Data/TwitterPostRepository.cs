using System.Diagnostics.CodeAnalysis;
using Dapper;
using important_game.infrastructure.Data.Connections;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Data;

[ExcludeFromCodeCoverage]
public class TwitterPostRepository(IDbConnectionFactory connectionFactory) : ITwitterPostRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    public async Task<TwitterPostEntity?> GetByMatchIdAsync(int matchId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TwitterPostEntity>(
            TwitterPostQueries.SelectByMatchId,
            new { MatchId = matchId });
    }

    public async Task<int> GetPostCountForMonthAsync(DateTimeOffset date)
    {
        var startOfMonth = new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset);
        var endOfMonth = startOfMonth.AddMonths(1);

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            TwitterPostQueries.CountPostsInMonth,
            new
            {
                StartOfMonth = startOfMonth.ToString("yyyy-MM-dd HH:mm:ss"),
                EndOfMonth = endOfMonth.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task SaveTwitterPostAsync(TwitterPostEntity entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            TwitterPostQueries.InsertTwitterPost,
            new
            {
                entity.MatchId,
                entity.TweetId,
                entity.Content,
                PostedDateUTC = entity.PostedDateUTC.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }
}
