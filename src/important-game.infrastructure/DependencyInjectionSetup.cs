using important_game.infrastructure.Contexts.BroadcastChannels;
using important_game.infrastructure.Contexts.BroadcastChannels.Data;
using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.ExternalServices;
using important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Utils;
using important_game.infrastructure.Contexts.ScoreCalculator;
using important_game.infrastructure.Contexts.Teams.Data;
using important_game.infrastructure.Contexts.Users;
using important_game.infrastructure.Contexts.Users.Data;
using important_game.infrastructure.Data.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net;

namespace important_game.infrastructure;

public static class DependencyInjectionSetup
{
    public static IServiceCollection MatchImportanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add memory cache for SofaScore integration
        services.AddMemoryCache();

        services.Configure<FootballDataOptions>(configuration.GetSection("FootballData"));
        services.Configure<LiveScoreCalculatorOptions>(configuration.GetSection("LiveScoreCalculator"));
        services.AddHttpClient<IExternalIntegrationProvider, FootballDataProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<FootballDataOptions>>().Value;
            client.BaseAddress = new Uri(FootballDataConstants.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                if (client.DefaultRequestHeaders.Contains("X-Auth-Token"))
                {
                    client.DefaultRequestHeaders.Remove("X-Auth-Token");
                }

                client.DefaultRequestHeaders.Add("X-Auth-Token", options.ApiKey);
            }
        });

        //services.AddScoped<IExcitmentMatchProcessor, ExcitementMatchProcessor>();
        //services.AddScoped<IExcitmentMatchLiveProcessor, ExcitmentMatchLiveProcessor>();
        //services.AddScoped<IExcitmentMatchService, ExcitmentMatchService>();

        //services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));
        //services.AddHttpClient<ITelegramBot, TelegramBot>((sp, client) =>
        //{
        //    var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
        //    if (!string.IsNullOrWhiteSpace(options.BotToken))
        //    {
        //        client.BaseAddress = new Uri($"https://api.telegram.org/bot{options.BotToken}/");
        //    }
        //});

        // Register Dapper database infrastructure
        // Connection factory for database access
        services.AddScoped<IDbConnectionFactory>(sp =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            return new SqliteConnectionFactory(connectionString ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found"));
        });

        // Register specialized repositories following Single Responsibility Principle
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IExternalProvidersRepository, ExternalProvidersRepository>();
        services.AddScoped<IMatchesRepository, MatchesRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBroadcastChannelRepository, BroadcastChannelRepository>();

        // Register external data synchronization service
        services.AddScoped<IExternalCompetitionSyncService, ExternalCompetitionSyncService>();
        services.AddScoped<IExternalMatchesSyncService, ExternalMatchesSyncService>();
        services.AddScoped<IIntegrationProviderFactory, IntegrationProviderFactory>();
        services.AddScoped<IExternalProviderSettings, ExternalProviderSettings>();

        // Registar match calculators
        services.AddScoped<IMatchCalculatorOrchestrator, MatchCalculatorOrchestrator>();
        services.AddScoped<IMatchCalculator, MatchCalculator>();
        services.AddScoped<LiveScoreCalculator>();

        // Register matches services
        services.AddScoped<IMatchService, MatchService>();

        // Register user services
        services.AddScoped<IUserService, UserService>();

        // Register broadcast channel services
        services.AddScoped<IBroadcastChannelService, BroadcastChannelService>();
        services.AddScoped<IBroadcastOrchestrator, BroadcastOrchestrator>();
        services.AddScoped<ITvGuideMatcher, TvGuideMatcher>();

        // Register SofaScore integration with optimized HttpClient
        services.AddSingleton<SofaScoreRateLimiter>();
        services.AddHttpClient<ISofaScoreIntegration, SofaScoreIntegration>((sp, client) =>
        {
            // Configure base settings
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Critical headers to simulate a real browser and avoid Cloudflare blocking
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,pt;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Referer", "https://www.sofascore.com/");
            client.DefaultRequestHeaders.Add("Origin", "https://www.sofascore.com");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Pragma", "no-cache");

            // Sec-Fetch headers to simulate CORS requests from the browser
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // Connection pooling and automatic decompression
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,

            // Keep connection alive to reduce overhead
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 2
        });

        return services;
    }
}

