namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for CompetitionTable repository operations.
    /// </summary>
    internal static class CompetitionTableQueries
    {
        internal const string CheckCompetitionTableExists = 
            "SELECT COUNT(*) FROM competition_table WHERE competition_id = @CompetitionId AND team_id = @TeamId";

        internal const string InsertCompetitionTable = @"
            INSERT INTO competition_table (
                competition_id, team_id, position, points, matches, wins, draws, losses,
                goals_for, goals_against
            ) VALUES (
                @CompetitionId, @TeamId, @Position, @Points, @Matches, @Wins, @Draws, @Losses,
                @GoalsFor, @GoalsAgainst
            )";

        internal const string UpdateCompetitionTable = @"
            UPDATE competition_table 
            SET position = @Position,
                points = @Points,
                matches = @Matches,
                wins = @Wins,
                draws = @Draws,
                losses = @Losses,
                goals_for = @GoalsFor,
                goals_against = @GoalsAgainst,
                updated_at = @UpdatedAt
            WHERE competition_id = @CompetitionId AND team_id = @TeamId";

        internal const string SelectCompetitionTableByCompetitionId = @"
            SELECT 
                id AS Id,
                competition_id AS CompetitionId,
                team_id AS TeamId,
                position AS Position,
                points AS Points,
                matches AS Matches,
                wins AS Wins,
                draws AS Draws,
                losses AS Losses,
                goals_for AS GoalsFor,
                goals_against AS GoalsAgainst,
                updated_at AS UpdatedAt
            FROM competition_table
            WHERE competition_id = @CompetitionId
            ORDER BY position ASC";

        internal const string SelectLastCompetitionTableUpdate = @"
            SELECT MAX(updated_at) AS LastUpdate
            FROM competition_table
            WHERE competition_id = @CompetitionId";

        internal const string DeleteCompetitionTableByCompetitionId = 
            "DELETE FROM competition_table WHERE competition_id = @CompetitionId";

        internal const string SelectCompetitionTableByTeamAndCompetition = @"
            SELECT 
                id AS Id,
                competition_id AS CompetitionId,
                team_id AS TeamId,
                position AS Position,
                points AS Points,
                matches AS Matches,
                wins AS Wins,
                draws AS Draws,
                losses AS Losses,
                goals_for AS GoalsFor,
                goals_against AS GoalsAgainst,
                updated_at AS UpdatedAt
            FROM competition_table
            WHERE competition_id = @CompetitionId AND team_id = @TeamId";
    }
}
