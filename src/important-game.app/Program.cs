using Dapper;
using important_game.app.Handlers;
using important_game.infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLitePCL;

Batteries.Init();

var builder = Host.CreateDefaultBuilder(args);

// Configure the host for long-running background services
builder.ConfigureServices((context, services) =>
{
    services.AddLogging(configure =>
    {
        configure.AddConsole();
        configure.AddDebug();
    });

    services.MatchImportanceInfrastructure(context.Configuration);

    // Register all background jobs
    services.AddHostedService<MatchCalculatorJob>();
    services.AddHostedService<SyncCompetitionJob>();
    services.AddHostedService<SyncFinishedMatchesJob>();
    services.AddHostedService<SyncUpcomingMatchesJob>();
});

// Add Dapper type handlers
SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
SqlMapper.AddTypeHandler(new NullableDateTimeOffsetHandler());

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started successfully with {JobCount} background jobs registered.", 4);
logger.LogInformation("Press Ctrl+C to stop the application.");

try
{
    await host.RunAsync();
}
catch (OperationCanceledException)
{
    logger.LogInformation("Application is shutting down gracefully.");
}
finally
{
    logger.LogInformation("Application has stopped.");
}
