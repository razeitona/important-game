using important_game.ui.Domain.ImportantMatch;
using important_game.ui.Domain.LeagueInformation;
using important_game.ui.Domain.SofaScoreAPI;
using important_game.ui.Infrastructure.ImportantMatch;
using Microsoft.Extensions.DependencyInjection;

namespace important_game.ui
{
    public static class DependencyInjectionSetup
    {

        public static IServiceCollection MatchImportanceDependency(this IServiceCollection services)
        {
            services.AddHttpClient<ISofaScoreIntegration, SofaScoreIntegration>();
            services.AddScoped<ILeagueProcessor, SofaScoreLeagueProcessor>();
            services.AddScoped<IExcitmentMatchProcessor, ExcitementMatchProcessor>();

            return services;
        }
    }
}
