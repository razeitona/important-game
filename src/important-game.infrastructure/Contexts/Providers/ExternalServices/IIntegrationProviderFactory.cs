namespace important_game.infrastructure.Contexts.Providers.ExternalServices;
public interface IIntegrationProviderFactory
{
    T GetBest<T>();
}

internal class IntegrationProviderFactory(IEnumerable<IExternalIntegrationProvider> providers) : IIntegrationProviderFactory
{
    public T GetBest<T>()
    {
        var provider = providers.OfType<T>().FirstOrDefault();
        if (provider == null)
            throw new NotImplementedException();
        return provider;
    }
}
