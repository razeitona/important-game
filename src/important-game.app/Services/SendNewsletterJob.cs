using important_game.infrastructure.Contexts.Newsletter;
using important_game.infrastructure.Contexts.Newsletter.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace important_game.app.Services;

/// <summary>
/// Background job that sends weekly newsletter emails every Sunday at 22:00 UTC.
/// The newsletter contains the top 5 most exciting matches for the upcoming week.
/// </summary>
public class SendNewsletterJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SendNewsletterJob> _logger;
    private readonly EmailOptions _options;

    public SendNewsletterJob(
        IServiceProvider serviceProvider,
        IOptions<EmailOptions> options,
        ILogger<SendNewsletterJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("SendNewsletterJob is disabled via configuration");
            return;
        }

        _logger.LogInformation("SendNewsletterJob started. Configured to run on {Day} at {Hour}:00 UTC",
            (DayOfWeek)_options.SendDayOfWeek, _options.SendHourUtc);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextRunTime = CalculateNextRunTime();
                var delay = nextRunTime - DateTimeOffset.UtcNow;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("Next newsletter send scheduled for {NextRunTime} (in {Hours} hours)",
                        nextRunTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"), delay.TotalHours.ToString("F1"));

                    // Wait until the scheduled time
                    await Task.Delay(delay, stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                // Execute the job
                await RunJobAsync(stoppingToken);

                // Wait a short time before calculating the next run to avoid running twice
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SendNewsletterJob is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendNewsletterJob main loop. Will retry in 1 hour.");

                // Wait before retrying to avoid tight error loops
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task RunJobAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting newsletter send job execution at {Time}", DateTimeOffset.UtcNow);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<INewsletterOrchestrator>();

            await orchestrator.SendWeeklyNewsletterAsync(stoppingToken);

            _logger.LogInformation("Newsletter send job completed successfully at {Time}", DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute newsletter send job");
            throw;
        }
    }

    /// <summary>
    /// Calculates the next time the newsletter should be sent based on configuration.
    /// Default: Sunday at 22:00 UTC
    /// </summary>
    private DateTimeOffset CalculateNextRunTime()
    {
        var now = DateTimeOffset.UtcNow;
        var targetDayOfWeek = (DayOfWeek)_options.SendDayOfWeek;
        var targetHour = _options.SendHourUtc;

        // Start with today at the target hour
        var targetTime = new DateTimeOffset(
            now.Year, now.Month, now.Day,
            targetHour, 0, 0,
            TimeSpan.Zero);

        // Calculate days until next occurrence of target day
        int daysUntilTargetDay = ((int)targetDayOfWeek - (int)now.DayOfWeek + 7) % 7;

        // If we're on the target day but past the target time, go to next week
        if (daysUntilTargetDay == 0 && now.Hour >= targetHour)
        {
            daysUntilTargetDay = 7;
        }

        return targetTime.AddDays(daysUntilTargetDay);
    }
}
