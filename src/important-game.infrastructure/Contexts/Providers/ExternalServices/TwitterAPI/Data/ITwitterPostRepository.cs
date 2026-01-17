namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Data;

public interface ITwitterPostRepository
{
    Task<TwitterPostEntity?> GetByMatchIdAsync(int matchId);
    Task<int> GetPostCountForMonthAsync(DateTimeOffset date);
    Task SaveTwitterPostAsync(TwitterPostEntity entity);
}
