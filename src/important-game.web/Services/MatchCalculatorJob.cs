using important_game.infrastructure.ImportantMatch;

public class MatchCalculatorJob : BackgroundService
{

    private readonly IServiceProvider _serviceProvider;
    public MatchCalculatorJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var matchProcessor = scope.ServiceProvider.GetRequiredService<IExcitmentMatchService>();

                        await matchProcessor.CalculateUpcomingMatchsExcitment();
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
