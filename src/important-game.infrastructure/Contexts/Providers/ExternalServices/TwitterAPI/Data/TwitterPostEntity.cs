namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Data;

public class TwitterPostEntity
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public string? TweetId { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset PostedDateUTC { get; set; }
}
