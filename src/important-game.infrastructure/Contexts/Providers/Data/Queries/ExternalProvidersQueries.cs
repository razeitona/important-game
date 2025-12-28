namespace important_game.infrastructure.Contexts.Providers.Data.Queries;

/// <summary>
/// SQL queries for ExternalProviderTeam repository operations.
/// </summary>
internal static class ExternalProvidersQueries
{
    internal const string SelectExternalProviderById = @"
            SELECT 
                ProviderId,
                Name,
                MaxRequestsPerSecond,
                MaxRequestsPerMinute,
                MaxRequestsPerHour,
                MaxRequestsPerDay ,
                MaxRequestsPerMonth
            FROM ExternalProviders
            WHERE ProviderId = @ProviderId";

    internal const string SelectExternalProviderLogsById = @"
            SELECT 
                ProviderId,
                RequestPath,
                RequestDate
            FROM ExternalProvidersLogs
            WHERE ProviderId = @ProviderId";

    internal const string InsertExternalProviderLog = @"
            INSERT INTO ExternalProvidersLogs (ProviderId, RequestPath, RequestDate)
            VALUES (@ProviderId, @RequestPath, @RequestDate)";
}
