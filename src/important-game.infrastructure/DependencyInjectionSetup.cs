using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Data;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.LeagueProcessors;
using important_game.infrastructure.SofaScoreAPI;
using important_game.infrastructure.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace important_game.infrastructure
{
    public static class DependencyInjectionSetup
    {

        public static IServiceCollection MatchImportanceInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddScoped<ISofaScoreIntegration, SofaScoreIntegration>();
            services.AddScoped<IExcitmentMatchProcessor, ExcitementMatchProcessor>();
            services.AddScoped<IExcitmentMatchLiveProcessor, ExcitmentMatchLiveProcessor>();
            services.AddScoped<IExctimentMatchRepository, ExcitmentMatchRepository>();
            services.AddScoped<IExcitmentMatchService, ExcitmentMatchService>();
            services.AddScoped<ILeagueProcessor, SofaScoreLeagueProcessor>();
            services.AddScoped<TelegramBot, TelegramBot>();

            services.AddDbContext<ImportantMatchDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
