namespace important_game.infrastructure.Contexts.Providers.Data.Queries;

/// <summary>
/// SQL queries for ExternalIntegrationCompetitions repository operations.
/// </summary>
internal static class ExternalProviderCompetitionSeasonsQueries
{
    internal const string CheckExternalProviderSeasonExists =
        "SELECT COUNT(*) FROM ExternalProviderCompetitionSeasons WHERE ProviderId = @ProviderId AND InternalSeasonId = @InternalSeasonId";

    internal const string InsertExternalProviderSeason = @"
            INSERT INTO ExternalProviderCompetitionSeasons (ProviderId, InternalSeasonId, ExternalSeasonId)
            VALUES (@ProviderId, @InternalSeasonId, @ExternalSeasonId)";

    internal const string UpdateExternalProviderSeason = @"
            UPDATE ExternalProviderCompetitionSeasons 
            SET ExternalSeasonId = @ExternalSeasonId
            WHERE ProviderId = @ProviderId AND InternalSeasonId = @InternalSeasonId";

    internal const string SelectExternalProviderCompetitionSeasonByInternalId = @"
            SELECT 
                ProviderId,
                InternalSeasonId,
                ExternalSeasonId
            FROM ExternalProviderCompetitionSeasons
            WHERE ProviderId = @ProviderId AND InternalSeasonId = @InternalSeasonId";

    internal const string SelectExternalProviderCompetitionSeasonByExternalId = @"
            SELECT 
                ProviderId,
                InternalSeasonId,
                ExternalSeasonId
            FROM ExternalProviderCompetitionSeasons
            WHERE ProviderId = @ProviderId AND ExternalSeasonId = @ExternalSeasonId";

    internal const string SelectExternalProviderCompetitionSeasonsByProvider = @"
            SELECT 
                ProviderName,
                InternalSeasonId,
                ExternalSeasonId
            FROM ExternalProviderCompetitionSeasons
            WHERE ProviderId = @ProviderId
            ORDER BY InternalSeasonId";

    internal const string DeleteExternalProviderCompetitionSeason =
        "DELETE FROM ExternalProviderCompetitionSeasons WHERE ProviderId = @ProviderId AND InternalSeasonId = @InternalSeasonId";
}
