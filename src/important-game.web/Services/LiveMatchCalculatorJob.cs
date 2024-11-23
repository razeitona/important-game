using important_game.infrastructure.ImportantMatch;

public class LiveMatchCalculatorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public LiveMatchCalculatorJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var matchProcessor = scope.ServiceProvider.GetRequiredService<IExcitmentMatchService>();

                        await matchProcessor.CalculateUnfinishedMatchExcitment();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Handle graceful shutdown
        }
    }
}
