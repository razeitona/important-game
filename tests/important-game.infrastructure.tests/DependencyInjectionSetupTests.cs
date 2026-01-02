using FluentAssertions;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI;
using important_game.infrastructure.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace important_game.infrastructure.tests;

public class DependencyInjectionSetupTests
{
    [Fact]
    public void MatchImportanceInfrastructure_RegistersExpectedServices()
    {
        var config = new ConfigurationManager();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:"
        });

        var services = new ServiceCollection();
        services.AddLogging();

        services.MatchImportanceInfrastructure(config);

        services.Should().ContainSingle(d => d.ServiceType == typeof(ITelegramBot));
    }

    [Fact]
    public void MatchImportanceInfrastructure_ConfiguresHttpClient()
    {
        var config = new ConfigurationManager();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:"
        });

        var services = new ServiceCollection();
        services.AddLogging();
        services.MatchImportanceInfrastructure(config);

        var clientConfigurations = services
            .Where(d => d.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>)
                && d.ImplementationInstance is ConfigureNamedOptions<HttpClientFactoryOptions>)
            .Select(d => (ConfigureNamedOptions<HttpClientFactoryOptions>)d.ImplementationInstance!);

        clientConfigurations.Should().Contain(c => c.Name == typeof(ISofaScoreIntegration).FullName || c.Name == nameof(ISofaScoreIntegration));
    }
}
