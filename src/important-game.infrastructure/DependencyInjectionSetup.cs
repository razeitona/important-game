using System;
using System.Net.Http.Headers;
using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Data;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.LeagueProcessors;
using important_game.infrastructure.SofaScoreAPI;
using important_game.infrastructure.SofaScoreAPI.Models;
using important_game.infrastructure.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            services.AddScoped<IExcitmentMatchProcessor, ExcitementMatchProcessor>();
            services.AddScoped<IExcitmentMatchLiveProcessor, ExcitmentMatchLiveProcessor>();
            services.AddScoped<IExctimentMatchRepository, ExcitmentMatchRepository>();
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

            services.AddDbContext<ImportantMatchDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
