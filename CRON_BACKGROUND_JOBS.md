# Cron-Based Background Jobs

This project uses cron expressions for scheduling background tasks instead of fixed time intervals. This provides more flexible and predictable scheduling.

## Overview

The `CronBackgroundService` base class in the infrastructure layer provides a reusable foundation for cron-scheduled background jobs. Services inherit from this class and only need to implement the actual work logic.

## Benefits of Cron-Based Scheduling

### vs. Timer-Based (PeriodicTimer)
- **Predictable Schedules**: Run at specific times (e.g., "2:00 AM every day") instead of intervals
- **Flexible Patterns**: Complex schedules like "Every Monday at 9 AM" or "First day of month"
- **Standard Format**: Industry-standard cron syntax used by Linux, Kubernetes, and most scheduling systems
- **Easier Configuration**: Non-developers can modify schedules in appsettings.json
- **Better for Maintenance Windows**: Align with low-traffic periods precisely

### Examples
```csharp
// Before: PeriodicTimer
using var timer = new PeriodicTimer(TimeSpan.FromDays(7));
while (await timer.WaitForNextTickAsync(stoppingToken))
{
    await DoWork();
}

// After: Cron-based
public class MyService : CronBackgroundService
{
    public MyService(ILogger<CronBackgroundService> logger)
        : base("0 0 2 * * *", logger) // Daily at 2:00 AM
    {
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // Your work here
    }
}
```

## Cron Expression Format

This project uses the **6-field format** (includes seconds):

```
┌─────────────  second (0 - 59)
│ ┌───────────  minute (0 - 59)
│ │ ┌─────────  hour (0 - 23)
│ │ │ ┌───────  day of month (1 - 31)
│ │ │ │ ┌─────  month (1 - 12)
│ │ │ │ │ ┌───  day of week (0 - 6) (Sunday = 0)
│ │ │ │ │ │
│ │ │ │ │ │
* * * * * *
```

### Common Examples

| Schedule | Cron Expression | Description |
|----------|----------------|-------------|
| Every day at 2:00 AM | `0 0 2 * * *` | Default for sitemap generator |
| Every hour | `0 0 * * * *` | Hourly on the hour |
| Every 30 minutes | `0 */30 * * * *` | Every 30 minutes |
| Every Monday at 9 AM | `0 0 9 * * 1` | Weekly on Monday |
| First of month at midnight | `0 0 0 1 * *` | Monthly |
| Every 5 minutes | `0 */5 * * * *` | Frequent updates |
| Weekdays at 6 PM | `0 0 18 * * 1-5` | Monday-Friday |
| Every Sunday at 3 AM | `0 0 3 * * 0` | Weekly maintenance |

### Special Characters

- `*` - Any value (every)
- `,` - List of values (`1,3,5` = 1st, 3rd, 5th)
- `-` - Range (`1-5` = 1 through 5)
- `/` - Step values (`*/15` = every 15)

## Current Background Jobs

### 1. SitemapGeneratorService

**Purpose**: Generates sitemap.xml and llms.txt files for SEO and AI crawlers

**Schedule**: Daily at 2:00 AM UTC (`0 0 2 * * *`)

**Configuration**:
```json
{
  "BackgroundJobs": {
    "SitemapGenerator": {
      "CronExpression": "0 0 2 * * *"
    }
  }
}
```

**Location**: `src/important-game.web/Services/SitemapGeneratorService.cs`

**What it does**:
1. Fetches all upcoming matches from database
2. Generates XML sitemap with static pages and match detail pages
3. Generates llms.txt file for AI crawler discovery
4. Writes both files to wwwroot directory

**Runs on startup**: Yes (after 10-second delay)

## Base Class: CronBackgroundService

**Location**: `src/important-game.infrastructure/Services/CronBackgroundService.cs`

### Features

- **Automatic Scheduling**: Calculates next run time based on cron expression
- **Startup Execution**: Optionally runs immediately on app startup
- **Comprehensive Logging**: Logs start, completion, duration, and errors
- **Timezone Support**: Runs in specified timezone (default: UTC)
- **Graceful Shutdown**: Handles cancellation tokens properly

### Protected Properties

```csharp
// Override in derived class if needed
protected virtual bool RunOnStartup => true;
protected virtual TimeSpan StartupDelay => TimeSpan.FromSeconds(10);
```

### Abstract Method

```csharp
// Implement this in your service
protected abstract Task DoWorkAsync(CancellationToken stoppingToken);
```

## Creating a New Cron Job

### Step 1: Create the Service

```csharp
using important_game.infrastructure.Services;

namespace important_game.web.Services;

public class MyScheduledService : CronBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public MyScheduledService(
        IServiceProvider serviceProvider,
        ILogger<CronBackgroundService> logger,
        IConfiguration configuration)
        : base(
            cronExpression: configuration["BackgroundJobs:MyService:CronExpression"]
                ?? "0 0 * * * *", // Default: hourly
            logger: logger,
            timeZone: TimeZoneInfo.Utc)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // Use scoped services
        using var scope = _serviceProvider.CreateScope();
        var myService = scope.ServiceProvider.GetRequiredService<IMyService>();

        // Do your work
        await myService.ProcessDataAsync(stoppingToken);
    }
}
```

### Step 2: Register the Service

In `Program.cs`:
```csharp
builder.Services.AddHostedService<MyScheduledService>();
```

### Step 3: Configure the Schedule

In `appsettings.json`:
```json
{
  "BackgroundJobs": {
    "MyService": {
      "CronExpression": "0 */15 * * * *"
    }
  }
}
```

### Step 4: Override Options (Optional)

```csharp
public class MyScheduledService : CronBackgroundService
{
    // Don't run on startup
    protected override bool RunOnStartup => false;

    // Or change the delay
    protected override TimeSpan StartupDelay => TimeSpan.FromMinutes(1);

    // ... rest of implementation
}
```

## Testing Cron Expressions

### Online Tools
- [Crontab Guru](https://crontab.guru/) - Human-readable cron descriptions
- [Cronitor Cron Expression Descriptor](https://cronitor.io/cron-reference) - Full reference guide

### Testing in Code

```csharp
using Cronos;

var expression = CronExpression.Parse("0 0 2 * * *", CronFormat.IncludeSeconds);
var now = DateTimeOffset.UtcNow;
var next = expression.GetNextOccurrence(now, TimeZoneInfo.Utc);

Console.WriteLine($"Next run: {next}");
```

### Quick Test Schedule

For testing, use a frequent schedule:
```json
{
  "BackgroundJobs": {
    "MyService": {
      "CronExpression": "0 */1 * * * *"  // Every minute
    }
  }
}
```

**Remember to change back to production schedule!**

## Logging

All cron jobs log the following:

1. **Service Start**:
   ```
   Initialized SitemapGeneratorService with cron expression '0 0 2 * * *' in timezone 'UTC'
   ```

2. **Next Schedule**:
   ```
   SitemapGeneratorService next execution scheduled at 2026-01-07T02:00:00+00:00 (in 04:15:30)
   ```

3. **Execution Start**:
   ```
   SitemapGeneratorService execution started at 2026-01-07T02:00:00+00:00
   ```

4. **Execution Complete**:
   ```
   SitemapGeneratorService execution completed successfully in 00:00:02.1234567
   ```

5. **Errors**:
   ```
   SitemapGeneratorService execution failed after 00:00:01.5
   ```

## Monitoring

### Check Service Status

View logs to see:
- When the service started
- When it last ran
- When it will run next
- Any errors or exceptions

### Application Insights (if configured)

Logs are automatically sent to Application Insights with:
- Custom dimensions for service name
- Duration tracking
- Exception tracking
- Success/failure metrics

## Best Practices

### 1. Use Scoped Services
Always create a scope for dependency injection:
```csharp
using var scope = _serviceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IMyService>();
```

### 2. Handle Cancellation
Check cancellation token before long operations:
```csharp
protected override async Task DoWorkAsync(CancellationToken stoppingToken)
{
    if (stoppingToken.IsCancellationRequested)
        return;

    // Your work
}
```

### 3. Configure Schedules Externally
Always allow configuration override:
```csharp
: base(
    cronExpression: configuration["BackgroundJobs:MyService:CronExpression"] ?? "0 0 * * * *",
    logger: logger)
```

### 4. Choose Appropriate Times
- **Low Traffic**: 2-4 AM local time
- **Before Peak**: 6-8 AM for data prep
- **Off-Peak**: Weekends for heavy processing

### 5. Avoid Overlapping Runs
If a job might take longer than its schedule:
- Use longer intervals
- Add locking mechanism
- Skip if previous run still active

### 6. Document the Schedule
Always add XML comments:
```csharp
/// <summary>
/// Processes user analytics data.
/// Default: Daily at 3:00 AM UTC ("0 0 3 * * *")
/// </summary>
```

## Timezone Considerations

### Default: UTC
All schedules run in UTC by default for consistency:
```csharp
: base(cronExpression: "0 0 2 * * *", logger: logger, timeZone: TimeZoneInfo.Utc)
```

### Using Local Timezone
```csharp
: base(
    cronExpression: "0 0 2 * * *",
    logger: logger,
    timeZone: TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
```

### Multiple Timezones
Create separate services for different timezones if needed.

## Troubleshooting

### Job Not Running

1. **Check configuration**:
   ```json
   {
     "BackgroundJobs": {
       "MyService": {
         "CronExpression": "0 0 2 * * *"  // Valid?
       }
     }
   }
   ```

2. **Check registration** in `Program.cs`:
   ```csharp
   builder.Services.AddHostedService<MyService>();
   ```

3. **Check logs** for:
   - Service initialization message
   - Next scheduled run time
   - Any exceptions

### Job Running at Wrong Time

1. **Verify timezone**: Check if UTC vs local time
2. **Test cron expression**: Use [Crontab Guru](https://crontab.guru/)
3. **Check server time**: Ensure server clock is correct

### Job Running Too Frequently

1. **Check cron format**: Using 6 fields (includes seconds)?
2. **Review expression**: `* * * * * *` runs every second!
3. **Validate configuration**: Check appsettings.json

### Job Failing

1. **Check exception logs**: See what's throwing
2. **Verify dependencies**: Are all services registered?
3. **Test scope creation**: Services must be scoped, not singleton
4. **Check permissions**: File write access, database access, etc.

## Migration from PeriodicTimer

If you have existing timer-based services:

### Before:
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
    await DoWork(stoppingToken);

    using var timer = new PeriodicTimer(TimeSpan.FromDays(7));
    while (await timer.WaitForNextTickAsync(stoppingToken))
    {
        await DoWork(stoppingToken);
    }
}
```

### After:
```csharp
public class MyService : CronBackgroundService
{
    public MyService(ILogger<CronBackgroundService> logger, IConfiguration config)
        : base(
            cronExpression: config["BackgroundJobs:MyService:CronExpression"] ?? "0 0 2 * * 0",
            logger: logger)
    {
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // Your work here
    }
}
```

## Dependencies

- **Cronos** (0.11.1): Cron expression parsing and scheduling
- **Microsoft.Extensions.Hosting.Abstractions** (8.0.0): Background service infrastructure

## References

- [Cronos GitHub](https://github.com/HangfireIO/Cronos)
- [Cron Expression Format](https://en.wikipedia.org/wiki/Cron)
- [ASP.NET Core Background Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
