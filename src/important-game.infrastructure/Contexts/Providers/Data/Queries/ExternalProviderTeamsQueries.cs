namespace important_game.infrastructure.Contexts.Providers.Data.Queries;

/// <summary>
/// SQL queries for ExternalProviderTeam repository operations.
/// </summary>
internal static class ExternalProviderTeamsQueries
{
    internal const string CheckExternalProviderTeamExists =
        "SELECT COUNT(*) FROM ExternalProviderTeams WHERE ProviderId = @ProviderId AND InternalTeamId = @InternalTeamId";

    internal const string InsertExternalProviderTeam = @"
            INSERT INTO ExternalProviderTeams (ProviderId, InternalTeamId, ExternalTeamId)
            VALUES (@ProviderId, @InternalTeamId, @ExternalTeamId)";

    internal const string UpdateExternalProviderTeam = @"
            UPDATE ExternalProviderTeams 
            SET ExternalTeamId = @ExternalTeamId
            WHERE ProviderId = @ProviderId AND InternalTeamId = @InternalTeamId";

    internal const string SelectExternalProviderTeamByIds = @"
            SELECT 
                ProviderId,
                InternalTeamId,
                ExternalTeamId
            FROM ExternalProviderTeams
            WHERE ProviderId = @ProviderId AND InternalTeamId = @InternalTeamId";

    internal const string SelectExternalProviderTeamByExternalId = @"
            SELECT 
                ProviderId,
                InternalTeamId,
                ExternalTeamId
            FROM ExternalProviderTeams
            WHERE ProviderId = @ProviderId AND ExternalTeamId = @ExternalTeamId";

    internal const string SelectExternalProviderTeamsByProvider = @"
            SELECT 
                ProviderId,
                InternalTeamId,
                ExternalTeamId
            FROM ExternalProviderTeams
            WHERE ProviderId = @ProviderId
            ORDER BY InternalTeamId";

    internal const string SelectExternalProviderTeamsByInternalTeamIds = @"
            SELECT 
                ProviderId,
                InternalTeamId,
                ExternalTeamId
            FROM ExternalProviderTeams
            WHERE ProviderId = @ProviderId AND InternalTeamId IN @InternalTeamIds
            ORDER BY InternalTeamId";

    internal const string DeleteExternalProviderTeam =
        "DELETE FROM ExternalProviderTeams WHERE ProviderId = @ProviderId AND InternalTeamId = @InternalTeamId";

    internal const string DeleteExternalProviderTeamsByProvider =
        "DELETE FROM ExternalProviderTeams WHERE ProviderId = @ProviderId";
}
