using important_game.infrastructure.Contexts.Providers.ExternalServices;
using important_game.infrastructure.Contexts.ScoreCalculator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class SyncUpcomingMatchesJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncUpcomingMatchesJob> _logger;

    public SyncUpcomingMatchesJob(IServiceProvider serviceProvider, ILogger<SyncUpcomingMatchesJob> logger)
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
            var matchProcessor = scope.ServiceProvider.GetRequiredService<IExternalMatchesSyncService>();
            var calculator = scope.ServiceProvider.GetRequiredService<IMatchCalculatorOrchestrator>();
            await matchProcessor.SyncUpcomingMatchesAsync().ConfigureAwait(false);
            await calculator.CalculateExcitmentScoreAsync(skipDateCondition : true).ConfigureAwait(false);
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

