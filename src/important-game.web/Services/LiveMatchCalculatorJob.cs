using important_game.infrastructure.ImportantMatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class LiveMatchCalculatorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LiveMatchCalculatorJob> _logger;

    public LiveMatchCalculatorJob(IServiceProvider serviceProvider, ILogger<LiveMatchCalculatorJob> logger)
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
            _logger.LogInformation("Live match calculator job is stopping.");
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
            var matchProcessor = scope.ServiceProvider.GetRequiredService<IExcitmentMatchService>();
            await matchProcessor.CalculateUnfinishedMatchExcitment().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate live match excitement.");
        }
    }
}

