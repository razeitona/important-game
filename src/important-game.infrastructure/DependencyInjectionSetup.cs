using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.ExternalServices;
using important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData;
using important_game.infrastructure.Contexts.ScoreCalculator;
using important_game.infrastructure.Contexts.Teams.Data;
using important_game.infrastructure.Contexts.Users;
using important_game.infrastructure.Contexts.Users.Data;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories;
using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace important_game.infrastructure;

public static class DependencyInjectionSetup
{
    public static IServiceCollection MatchImportanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FootballDataOptions>(configuration.GetSection("FootballData"));
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
        services.AddScoped<IMatchRepository, MatchRepositoryDapper>();
        services.AddScoped<ILiveMatchRepository, LiveMatchRepository>();
        services.AddScoped<IHeadToHeadRepository, HeadToHeadRepository>();
        services.AddScoped<IExternalProvidersRepository, ExternalProvidersRepository>();
        services.AddScoped<IMatchesRepository, MatchesRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Register external data synchronization service
        services.AddScoped<IExternalCompetitionSyncService, ExternalCompetitionSyncService>();
        services.AddScoped<IExternalMatchesSyncService, ExternalMatchesSyncService>();
        services.AddScoped<IIntegrationProviderFactory, IntegrationProviderFactory>();
        services.AddScoped<IExternalProviderSettings, ExternalProviderSettings>();

        // Registar match calculators
        services.AddScoped<IMatchCalculatorOrchestrator, MatchCalculatorOrchestrator>();
        services.AddScoped<IMatchCalculator, MatchCalculator>();

        // Register matches services
        services.AddScoped<IMatchService, MatchService>();

        // Register user services
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}

