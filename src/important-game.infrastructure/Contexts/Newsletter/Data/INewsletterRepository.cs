using important_game.infrastructure.Contexts.Newsletter.Data.Entities;

namespace important_game.infrastructure.Contexts.Newsletter.Data;

public interface INewsletterRepository
{
    Task<NewsletterSubscriberEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriberEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<int> CreateSubscriberAsync(NewsletterSubscriberEntity subscriber, CancellationToken cancellationToken = default);
    Task ReactivateSubscriberAsync(string email, CancellationToken cancellationToken = default);
    Task UnsubscribeAsync(string token, CancellationToken cancellationToken = default);
    Task<List<NewsletterSubscriberEntity>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default);
    Task<bool> IsEmailSubscribedAsync(string email, CancellationToken cancellationToken = default);
}
