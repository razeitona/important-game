using important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class SyncTwitterPostJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncTwitterPostJob> _logger;

    public SyncTwitterPostJob(IServiceProvider serviceProvider, ILogger<SyncTwitterPostJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunJobAsync(stoppingToken).ConfigureAwait(false);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await RunJobAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Match calculator job is stopping.");
        }
    }

    private async Task RunJobAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var twitter = scope.ServiceProvider.GetRequiredService<ITwitterOrchestrator>();
            await twitter.PostUpcomingExcitingMatchesAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Failed to calculate upcoming match excitement.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate upcoming match excitement.");
        }
    }
}

