using Dapper;
using important_game.infrastructure.Contexts.Newsletter.Data.Entities;
using important_game.infrastructure.Contexts.Newsletter.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Newsletter.Data;

[ExcludeFromCodeCoverage]
public class NewsletterRepository(IDbConnectionFactory connectionFactory) : INewsletterRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory
        ?? throw new ArgumentNullException(nameof(connectionFactory));

    public async Task<NewsletterSubscriberEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<NewsletterSubscriberEntity>(
            NewsletterQueries.GetSubscriberByEmail,
            new { Email = email.ToLowerInvariant() });
    }

    public async Task<NewsletterSubscriberEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<NewsletterSubscriberEntity>(
            NewsletterQueries.GetSubscriberByToken,
            new { UnsubscribeToken = token });
    }

    public async Task<int> CreateSubscriberAsync(NewsletterSubscriberEntity subscriber, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            NewsletterQueries.CreateSubscriber,
            new
            {
                Email = subscriber.Email.ToLowerInvariant(),
                SubscribedDateUTC = subscriber.SubscribedDateUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                subscriber.UnsubscribeToken
            });
    }

    public async Task ReactivateSubscriberAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            NewsletterQueries.ReactivateSubscriber,
            new
            {
                Email = email.ToLowerInvariant(),
                SubscribedDateUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task UnsubscribeAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            NewsletterQueries.Unsubscribe,
            new
            {
                UnsubscribeToken = token,
                UnsubscribedDateUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task<List<NewsletterSubscriberEntity>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<NewsletterSubscriberEntity>(
            NewsletterQueries.GetActiveSubscribers);
        return result.ToList();
    }

    public async Task<bool> IsEmailSubscribedAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            NewsletterQueries.IsEmailSubscribed,
            new { Email = email.ToLowerInvariant() });
        return count > 0;
    }
}
