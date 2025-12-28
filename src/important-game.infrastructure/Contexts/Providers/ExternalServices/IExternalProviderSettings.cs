using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.Data.Entities;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices;
public interface IExternalProviderSettings
{
    Task<List<ExternalProvidersLogsEntity>> GetLogsAsync(int providerId);
    Task SaveLogAsync(ExternalProvidersLogsEntity log);
    Task<ExternalProvidersEntity> GetProvidersEntityAsync(int providerId);

}

internal class ExternalProviderSettings(IExternalProvidersRepository repository) : IExternalProviderSettings
{
    public async Task<List<ExternalProvidersLogsEntity>> GetLogsAsync(int providerId)
    {
        var providerLogs = await repository.GetExternalProviderLogsByIdAsync(providerId);
        return providerLogs;
    }

    public async Task<ExternalProvidersEntity> GetProvidersEntityAsync(int providerId)
    {
        var provider = await repository.GetExternalProviderByIdAsync(providerId);
        return provider;
    }

    public async Task SaveLogAsync(ExternalProvidersLogsEntity entity)
    {
        await repository.SaveExternalProviderLogAsync(entity);
    }
}
