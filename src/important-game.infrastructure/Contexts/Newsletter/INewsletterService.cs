namespace important_game.infrastructure.Contexts.Newsletter;

public interface INewsletterService
{
    Task<NewsletterSubscriptionResult> SubscribeAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UnsubscribeAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsSubscribedAsync(string email, CancellationToken cancellationToken = default);
}

public class NewsletterSubscriptionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool AlreadySubscribed { get; set; }
}
