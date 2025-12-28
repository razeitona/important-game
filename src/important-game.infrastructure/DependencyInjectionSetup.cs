using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.ExternalServices;
using important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models;
using important_game.infrastructure.Contexts.Teams.Data;
using important_game.infrastructure.Data.Connections;
using important_game.infrastructure.Data.Repositories;
using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.LeagueProcessors;
using important_game.infrastructure.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using static System.Formats.Asn1.AsnWriter;

namespace important_game.infrastructure
{
    public static class DependencyInjectionSetup
    {
        public static IServiceCollection MatchImportanceInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddHttpClient<ISofaScoreIntegration, SofaScoreIntegration>(client =>
            {
                client.BaseAddress = new Uri(SofaScoreConstants.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
                client.DefaultRequestHeaders.Referrer = new Uri(SofaScoreConstants.BaseUrl);
                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            });

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

            services.AddScoped<IExcitmentMatchProcessor, ExcitementMatchProcessor>();
            services.AddScoped<IExcitmentMatchLiveProcessor, ExcitmentMatchLiveProcessor>();
            services.AddScoped<IExcitmentMatchService, ExcitmentMatchService>();
            services.AddScoped<ILeagueProcessor, SofaScoreLeagueProcessor>();

            services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));
            services.AddHttpClient<ITelegramBot, TelegramBot>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.BotToken))
                {
                    client.BaseAddress = new Uri($"https://api.telegram.org/bot{options.BotToken}/");
                }
            });

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
            services.AddScoped<IRivalryRepository, RivalryRepository>();
            services.AddScoped<IHeadToHeadRepository, HeadToHeadRepository>();
            services.AddScoped<IExternalProvidersRepository, ExternalProvidersRepository>();

            // Register external data synchronization service
            services.AddScoped<IExternalCompetitionSyncService, ExternalCompetitionSyncService>();
            services.AddScoped<IIntegrationProviderFactory, IntegrationProviderFactory>();
            services.AddScoped<IExternalProviderSettings, ExternalProviderSettings>();
           
            return services;
        }
    }
}

