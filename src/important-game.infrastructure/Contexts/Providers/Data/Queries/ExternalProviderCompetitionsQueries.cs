namespace important_game.infrastructure.Contexts.Providers.Data.Queries;

/// <summary>
/// SQL queries for ExternalIntegrationCompetitions repository operations.
/// </summary>
internal static class ExternalProviderCompetitionsQueries
{
    internal const string CheckExternalProviderCompetitionExists =
        "SELECT COUNT(*) FROM ExternalProviderCompetitions WHERE ProviderId = @ProviderId AND InternalCompetitionId = @InternalCompetitionId";

    internal const string InsertExternalProviderCompetition = @"
            INSERT INTO ExternalProviderCompetitions (ProviderId, InternalCompetitionId, ExternalCompetitionId)
            VALUES (@ProviderId, @InternalCompetitionId, @ExternalCompetitionId)";

    internal const string UpdateExternalProviderCompetition = @"
            UPDATE ExternalProviderCompetitions 
            SET ExternalCompetitionId = @ExternalCompetitionId
            WHERE ProviderId = @ProviderId AND InternalCompetitionId = @InternalCompetitionId";

    internal const string SelectExternalProviderCompetitionByInternalId = @"
            SELECT 
                ProviderId,
                InternalCompetitionId,
                ExternalCompetitionId
            FROM ExternalProviderCompetitions
            WHERE ProviderId = @ProviderId AND InternalCompetitionId = @InternalCompetitionId";

    internal const string SelectExternalCompetitionByExternalId = @"
            SELECT 
                ProviderId,
                InternalCompetitionId,
                ExternalCompetitionId
            FROM ExternalProviderCompetitions
            WHERE ProviderId = @ProviderId AND ExternalCompetitionId = @ExternalCompetitionId";

    internal const string SelectExternalProviderCompetitionsByProvider = @"
            SELECT 
                ProviderId,
                InternalCompetitionId,
                ExternalCompetitionId
            FROM ExternalProviderCompetitions
            WHERE ProviderId = @ProviderId
            ORDER BY InternalCompetitionId";

    internal const string DeleteExternalProviderCompetition =
    "DELETE FROM ExternalProviderCompetitionSeasons WHERE ProviderId = @ProviderId AND InternalSeasonId = @InternalSeasonId";
}
