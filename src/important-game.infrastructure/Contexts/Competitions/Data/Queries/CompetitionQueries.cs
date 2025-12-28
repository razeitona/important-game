namespace important_game.infrastructure.Contexts.Competitions.Data.Queries
{
    /// <summary>
    /// SQL queries for Competition repository operations.
    /// </summary>
    internal static class CompetitionQueries
    {
        internal const string CheckCompetitionExists = "SELECT COUNT(*) FROM Competitions WHERE CompetitionId = @CompetitionId";

        internal const string UpdateCompetition = @"
            UPDATE Competitions SET 
            Name = @Name,
            UpdatedAt = @UpdatedAt
            WHERE CompetitionId = @CompetitionId";

        internal const string InsertCompetition = @"
            INSERT INTO Competitions (CompetitionId, Name, PrimaryColor, BackgroundColor, LeagueRanking, IsActive, UpdatedAt)
            VALUES (@CompetitionId, @Name, @PrimaryColor, @BackgroundColor, @LeagueRanking, @IsActive, @UpdatedAt)";

        internal const string SelectCompetitionById = @"
            SELECT 
                CompetitionId,
                Name,
                PrimaryColor,
                BackgroundColor,
                LeagueRanking,
                IsActive,
                UpdatedAt
            FROM Competitions 
            WHERE id = @Id";

        internal const string SelectAllCompetitions = @"
            SELECT 
                CompetitionId,
                Name,
                PrimaryColor,
                BackgroundColor,
                LeagueRanking,
                IsActive,
                UpdatedAt
            FROM Competitions
            ORDER BY id";

        internal const string SelectActiveCompetitions = @"
            SELECT 
                CompetitionId,
                Name,
                PrimaryColor,
                BackgroundColor,
                LeagueRanking,
                IsActive,
                UpdatedAt
            FROM Competitions
            WHERE IsActive = 1
            ORDER BY LeagueRanking DESC";
    }
}
