using Dapper;
using important_game.app.Handlers;
using important_game.app.Services;
using important_game.infrastructure;
using important_game.infrastructure.Contexts.Newsletter.Email;
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

    // Configure Email options for the newsletter job
    services.Configure<EmailOptions>(context.Configuration.GetSection("Email"));

    // Register all background jobs
    services.AddHostedService<MatchCalculatorJob>();
    services.AddHostedService<LiveScoreCalculatorJob>();
    services.AddHostedService<SyncCompetitionJob>();
    services.AddHostedService<SyncFinishedMatchesJob>();
    services.AddHostedService<SyncUpcomingMatchesJob>();
    services.AddHostedService<SyncTwitterPostJob>();
    services.AddHostedService<SendNewsletterJob>();
    //services.AddHostedService<BroadcastFinderJob>();
});

// Add Dapper type handlers
SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
SqlMapper.AddTypeHandler(new NullableDateTimeOffsetHandler());

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started successfully with {JobCount} background jobs registered.", 7);
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
