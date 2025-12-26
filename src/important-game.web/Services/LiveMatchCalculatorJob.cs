using important_game.infrastructure.ImportantMatch;

namespace important_game.web.Services;

public class LiveMatchCalculatorJob(IServiceProvider serviceProvider, ILogger<LiveMatchCalculatorJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return;
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
            logger.LogInformation("Live match calculator job is stopping.");
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
            using var scope = serviceProvider.CreateScope();
            var matchProcessor = scope.ServiceProvider.GetRequiredService<IExcitmentMatchService>();
            await matchProcessor.CalculateUnfinishedMatchExcitment().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate live match excitement.");
        }
    }
}