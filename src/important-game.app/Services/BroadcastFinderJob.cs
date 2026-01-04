
using important_game.infrastructure.Contexts.BroadcastChannels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class BroadcastFinderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BroadcastFinderJob> _logger;

    public BroadcastFinderJob(IServiceProvider serviceProvider, ILogger<BroadcastFinderJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunJobAsync(stoppingToken).ConfigureAwait(false);

        using var timer = new PeriodicTimer(TimeSpan.FromDays(7));

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
            var broadcaster = scope.ServiceProvider.GetRequiredService<IBroadcastOrchestrator>();
            await broadcaster.ProcessBroadcastGuide().ConfigureAwait(false);
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

