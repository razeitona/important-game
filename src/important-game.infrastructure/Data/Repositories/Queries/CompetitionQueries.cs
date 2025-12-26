namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for Competition repository operations.
    /// </summary>
    internal static class CompetitionQueries
    {
        internal const string CheckCompetitionExists = "SELECT COUNT(*) FROM competition WHERE id = @Id";

        internal const string UpdateCompetition = @"
            UPDATE competition 
            SET name = @Name, code = @Code, title_holder_team_id = @TitleHolderTeamId
            WHERE id = @Id";

        internal const string InsertCompetition = @"
            INSERT INTO competition (id, name, code, primary_color, background_color, league_ranking, is_active, title_holder_team_id)
            VALUES (@Id, @Name, @Code, @PrimaryColor, @BackgroundColor, @LeagueRanking, @IsActive, @TitleHolderTeamId)";

        internal const string SelectCompetitionById = @"
            SELECT 
                id AS Id,
                name AS Name,
                code AS Code,
                primary_color AS PrimaryColor,
                background_color AS BackgroundColor,
                league_ranking AS LeagueRanking,
                is_active AS IsActive,
                title_holder_team_id AS TitleHolderTeamId
            FROM competition 
            WHERE id = @Id";

        internal const string SelectAllCompetitions = @"
            SELECT 
                id AS Id,
                name AS Name,
                code AS Code,
                primary_color AS PrimaryColor,
                background_color AS BackgroundColor,
                league_ranking AS LeagueRanking,
                is_active AS IsActive,
                title_holder_team_id AS TitleHolderTeamId
            FROM competition
            ORDER BY id";

        internal const string SelectActiveCompetitions = @"
            SELECT 
                id AS Id,
                name AS Name,
                code AS Code,
                primary_color AS PrimaryColor,
                background_color AS BackgroundColor,
                league_ranking AS LeagueRanking,
                is_active AS IsActive,
                title_holder_team_id AS TitleHolderTeamId,
                last_update_date AS LastUpdateDate
            FROM competition
            WHERE is_active = 1
            ORDER BY league_ranking DESC";
    }
}
