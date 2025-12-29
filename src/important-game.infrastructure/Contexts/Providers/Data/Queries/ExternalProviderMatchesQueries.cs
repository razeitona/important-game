namespace important_game.infrastructure.Contexts.Providers.Data.Queries;

/// <summary>
/// SQL queries for ExternalProviderTeam repository operations.
/// </summary>
internal static class ExternalProviderMatchesQueries
{
    internal const string InsertExternalProviderMatch = @"
                INSERT OR REPLACE INTO ExternalProviderMatches (ProviderId, InternalMatchId, ExternalMatchId)
                VALUES (@ProviderId, @InternalMatchId, @ExternalMatchId)";

    internal const string SelectExternalProviderMatchByIds = @"
                SELECT 
                    ProviderId,
                    InternalMatchId,
                    ExternalMatchId
                FROM ExternalProviderMatches
                WHERE ProviderId = @ProviderId AND InternalMatchId = @InternalMatchId";

    internal const string SelectExternalProviderMatchesByProvider = @"
                SELECT 
                    ProviderId,
                    InternalMatchId,
                    ExternalMatchId
                FROM ExternalProviderMatches
                WHERE ProviderId = @ProviderId
                ORDER BY InternalMatchId";

    internal const string DeleteExternalProviderMatch = @"
                DELETE FROM ExternalProviderMatches
                WHERE ProviderId = @ProviderId AND InternalMatchId = @InternalMatchId";
}
