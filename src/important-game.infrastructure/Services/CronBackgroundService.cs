using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace important_game.infrastructure.Services;

/// <summary>
/// Base class for cron-based background services.
/// Handles cron scheduling logic so derived classes only need to implement DoWorkAsync.
/// </summary>
public abstract class CronBackgroundService : BackgroundService
{
    private readonly ILogger<CronBackgroundService> _logger;
    private readonly CronExpression _cronExpression;
    private readonly TimeZoneInfo _timeZone;

    /// <summary>
    /// Creates a new cron-based background service.
    /// </summary>
    /// <param name="cronExpression">Cron expression (e.g., "0 2 * * *" for daily at 2:00 AM)</param>
    /// <param name="timeZone">Time zone for cron schedule (defaults to UTC)</param>
    /// <param name="logger">Logger instance</param>
    protected CronBackgroundService(
        string cronExpression,
        ILogger<CronBackgroundService> logger,
        TimeZoneInfo? timeZone = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cronExpression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
        _timeZone = timeZone ?? TimeZoneInfo.Utc;

        _logger.LogInformation(
            "Initialized {ServiceName} with cron expression '{CronExpression}' in timezone '{TimeZone}'",
            GetType().Name,
            cronExpression,
            _timeZone.Id);
    }

    /// <summary>
    /// Whether to run the task immediately on startup before waiting for the first cron schedule.
    /// Default is true.
    /// </summary>
    protected virtual bool RunOnStartup => true;

    /// <summary>
    /// Delay before running on startup (only used if RunOnStartup is true).
    /// Default is 10 seconds to allow application initialization.
    /// </summary>
    protected virtual TimeSpan StartupDelay => TimeSpan.FromSeconds(10);

    /// <summary>
    /// The work to be performed on the cron schedule.
    /// </summary>
    protected abstract Task DoWorkAsync(CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{ServiceName} is starting", GetType().Name);

        // Run on startup if configured
        if (RunOnStartup)
        {
            _logger.LogInformation(
                "{ServiceName} will run on startup after {Delay} delay",
                GetType().Name,
                StartupDelay);

            await Task.Delay(StartupDelay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteTaskAsync(stoppingToken);
            }
        }

        // Schedule based on cron expression
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextOccurrence = _cronExpression.GetNextOccurrence(now, _timeZone);

            if (!nextOccurrence.HasValue)
            {
                _logger.LogWarning(
                    "{ServiceName} has no next occurrence. Stopping service.",
                    GetType().Name);
                break;
            }

            var delay = nextOccurrence.Value - now;

            _logger.LogInformation(
                "{ServiceName} next execution scheduled at {NextRun} (in {Delay})",
                GetType().Name,
                nextOccurrence.Value,
                delay);

            try
            {
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteTaskAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("{ServiceName} is stopping", GetType().Name);
                break;
            }
        }
    }

    private async Task ExecuteTaskAsync(CancellationToken stoppingToken)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("{ServiceName} execution started at {StartTime}", GetType().Name, startTime);

            await DoWorkAsync(stoppingToken);

            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogInformation(
                "{ServiceName} execution completed successfully in {Duration}",
                GetType().Name,
                duration);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("{ServiceName} execution was cancelled", GetType().Name);
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger.LogError(
                ex,
                "{ServiceName} execution failed after {Duration}",
                GetType().Name,
                duration);
        }
    }
}
