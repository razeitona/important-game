using important_game.infrastructure.Contexts.Providers.ExternalServices;

public class SyncCompetitionJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncCompetitionJob> _logger;

    public SyncCompetitionJob(IServiceProvider serviceProvider, ILogger<SyncCompetitionJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunJobAsync(stoppingToken).ConfigureAwait(false);

        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

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
            var matchProcessor = scope.ServiceProvider.GetRequiredService<IExternalCompetitionSyncService>();
            await matchProcessor.SyncCompetitionsAsync().ConfigureAwait(false);
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

