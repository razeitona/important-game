using important_game.infrastructure.Contexts.ScoreCalculator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service that periodically calculates live excitement bonuses for ongoing matches.
/// Runs every 10 minutes (configurable) and updates matches with real-time statistics from SofaScore.
/// Uses priority-based updates to stay within rate limits.
/// </summary>
public class LiveScoreCalculatorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LiveScoreCalculatorJob> _logger;

    public LiveScoreCalculatorJob(IServiceProvider serviceProvider, ILogger<LiveScoreCalculatorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunJobAsync(stoppingToken).ConfigureAwait(false);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await RunJobAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Live score calculator job is stopping.");
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
            var calculator = scope.ServiceProvider.GetRequiredService<IMatchCalculatorOrchestrator>();
            await calculator.CalculateLiveScoreAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Live score calculation was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate live excitement scores.");
        }
    }
}
