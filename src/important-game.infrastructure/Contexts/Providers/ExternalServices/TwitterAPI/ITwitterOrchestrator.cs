namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;

public interface ITwitterOrchestrator
{
    Task PostUpcomingExcitingMatchesAsync(CancellationToken cancellationToken = default);
}
