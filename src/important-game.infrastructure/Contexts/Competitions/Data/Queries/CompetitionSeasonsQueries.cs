namespace important_game.infrastructure.Contexts.Competitions.Data.Queries
{
    /// <summary>
    /// SQL queries for CompetitionSeasons repository operations.
    /// </summary>
    internal static class CompetitionSeasonsQueries
    {
        internal const string CheckCompetitionSeasonExists =
            "SELECT COUNT(*) FROM CompetitionSeasons WHERE CompetitionId = @CompetitionId AND SeasonYear = @SeasonYear";

        internal const string InsertCompetitionSeason = @"
            INSERT INTO CompetitionSeasons (CompetitionId, SeasonYear, TitleHolderId)
            VALUES (@CompetitionId, @SeasonYear, @TitleHolderId)";

        internal const string UpdateCompetitionSeason = @"
            UPDATE CompetitionSeasons 
            SET TitleHolderId = @TitleHolderId
            WHERE SeasonId = @SeasonId";

        internal const string SelectCompetitionSeasonById = @"
            SELECT 
                SeasonId,
                CompetitionId,
                SeasonYear,
                NumberOfRounds,
                TitleHolderId,
                IsFinished,
                SyncStandingsDate
            FROM CompetitionSeasons
            WHERE SeasonId = @SeasonId";

        internal const string SelectCompetitionSeasonsByCompetition = @"
            SELECT 
                SeasonId,
                CompetitionId,
                SeasonYear,
                TitleHolderId
            FROM CompetitionSeasons
            WHERE CompetitionId = @CompetitionId
            ORDER BY SeasonYear DESC";

        internal const string SelectLatestCompetitionSeason = @"
            SELECT 
                SeasonId,
                CompetitionId,
                SeasonYear,
                TitleHolderId,
                IsFinished,
                SyncStandingsDate
            FROM CompetitionSeasons
            WHERE CompetitionId = @CompetitionId
            ORDER BY SeasonYear DESC
            LIMIT 1";

        internal const string SelectCompetitionSeasonByCompetitionAndYear = @"
            SELECT 
                SeasonId,
                CompetitionId,
                SeasonYear,
                TitleHolderId,
                IsFinished,
                SyncStandingsDate
            FROM CompetitionSeasons
            WHERE CompetitionId = @CompetitionId AND SeasonYear = @SeasonYear";

        internal const string DeleteCompetitionSeason =
            "DELETE FROM CompetitionSeasons WHERE SeasonId = @SeasonId";

        internal const string UpdateCompetitionSeasonStandingDate = @"
            UPDATE CompetitionSeasons 
            SET SyncStandingsDate = @SyncStandingsDate
            WHERE SeasonId = @SeasonId";
    }
}
