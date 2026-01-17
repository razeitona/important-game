using important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Models;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;

public interface ITwitterIntegration
{
    Task<TwitterPostResponse?> PostTweetAsync(string content);
    Task<bool> ValidateCredentialsAsync();
}
