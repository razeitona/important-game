namespace important_game.infrastructure.Contexts.Newsletter.Data.Queries;

public static class NewsletterQueries
{
    public const string GetSubscriberByEmail = @"
        SELECT Id, Email, SubscribedDateUTC, IsActive, UnsubscribedDateUTC, UnsubscribeToken
        FROM NewsletterSubscribers
        WHERE Email = @Email";

    public const string GetSubscriberByToken = @"
        SELECT Id, Email, SubscribedDateUTC, IsActive, UnsubscribedDateUTC, UnsubscribeToken
        FROM NewsletterSubscribers
        WHERE UnsubscribeToken = @UnsubscribeToken";

    public const string CreateSubscriber = @"
        INSERT INTO NewsletterSubscribers (Email, SubscribedDateUTC, IsActive, UnsubscribeToken)
        VALUES (@Email, @SubscribedDateUTC, 1, @UnsubscribeToken);
        SELECT last_insert_rowid();";

    public const string ReactivateSubscriber = @"
        UPDATE NewsletterSubscribers
        SET IsActive = 1,
            SubscribedDateUTC = @SubscribedDateUTC,
            UnsubscribedDateUTC = NULL
        WHERE Email = @Email";

    public const string Unsubscribe = @"
        UPDATE NewsletterSubscribers
        SET IsActive = 0,
            UnsubscribedDateUTC = @UnsubscribedDateUTC
        WHERE UnsubscribeToken = @UnsubscribeToken";

    public const string GetActiveSubscribers = @"
        SELECT Id, Email, SubscribedDateUTC, IsActive, UnsubscribedDateUTC, UnsubscribeToken
        FROM NewsletterSubscribers
        WHERE IsActive = 1
        ORDER BY SubscribedDateUTC DESC";

    public const string IsEmailSubscribed = @"
        SELECT COUNT(1)
        FROM NewsletterSubscribers
        WHERE Email = @Email AND IsActive = 1";
}
