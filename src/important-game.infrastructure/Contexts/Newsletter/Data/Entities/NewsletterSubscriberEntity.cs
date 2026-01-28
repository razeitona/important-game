namespace important_game.infrastructure.Contexts.Newsletter.Data.Entities;

public class NewsletterSubscriberEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public DateTime SubscribedDateUTC { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UnsubscribedDateUTC { get; set; }
    public required string UnsubscribeToken { get; set; }
}
