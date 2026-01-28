using important_game.infrastructure.Contexts.Newsletter.Data;
using important_game.infrastructure.Contexts.Newsletter.Data.Entities;
using System.Text.RegularExpressions;

namespace important_game.infrastructure.Contexts.Newsletter;

public partial class NewsletterService(INewsletterRepository newsletterRepository) : INewsletterService
{
    private readonly INewsletterRepository _newsletterRepository = newsletterRepository;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public async Task<NewsletterSubscriptionResult> SubscribeAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex().IsMatch(email))
        {
            return new NewsletterSubscriptionResult
            {
                Success = false,
                Message = "Please enter a valid email address."
            };
        }

        var normalizedEmail = email.ToLowerInvariant().Trim();

        var existing = await _newsletterRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existing != null)
        {
            if (existing.IsActive)
            {
                return new NewsletterSubscriptionResult
                {
                    Success = true,
                    AlreadySubscribed = true,
                    Message = "You're already subscribed to our newsletter!"
                };
            }

            await _newsletterRepository.ReactivateSubscriberAsync(normalizedEmail, cancellationToken);
            return new NewsletterSubscriptionResult
            {
                Success = true,
                Message = "Welcome back! You've been resubscribed to our newsletter."
            };
        }

        var subscriber = new NewsletterSubscriberEntity
        {
            Email = normalizedEmail,
            SubscribedDateUTC = DateTime.UtcNow,
            IsActive = true,
            UnsubscribeToken = Guid.NewGuid().ToString("N")
        };

        await _newsletterRepository.CreateSubscriberAsync(subscriber, cancellationToken);

        return new NewsletterSubscriptionResult
        {
            Success = true,
            Message = "Thank you for subscribing! You'll receive weekly updates about exciting matches."
        };
    }

    public async Task<bool> UnsubscribeAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var subscriber = await _newsletterRepository.GetByTokenAsync(token, cancellationToken);
        if (subscriber == null || !subscriber.IsActive)
            return false;

        await _newsletterRepository.UnsubscribeAsync(token, cancellationToken);
        return true;
    }

    public async Task<bool> IsSubscribedAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _newsletterRepository.IsEmailSubscribedAsync(email.ToLowerInvariant(), cancellationToken);
    }
}
