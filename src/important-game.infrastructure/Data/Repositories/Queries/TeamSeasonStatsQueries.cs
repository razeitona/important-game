namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for TeamSeasonStats repository operations.
    /// </summary>
    internal static class TeamSeasonStatsQueries
    {
        internal const string CheckTeamSeasonStatsExists = 
            "SELECT COUNT(*) FROM team_season_stats WHERE team_id = @TeamId AND competition_id = @CompetitionId";

        internal const string InsertTeamSeasonStats = @"
            INSERT INTO team_season_stats (
                team_id, competition_id, goals_for_5, goals_against_5, wins_5, draws_5, losses_5, updated_at
            ) VALUES (
                @TeamId, @CompetitionId, @GoalsFor5, @GoalsAgainst5, @Wins5, @Draws5, @Losses5, @UpdatedAt
            )";

        internal const string UpdateTeamSeasonStats = @"
            UPDATE team_season_stats 
            SET goals_for_5 = @GoalsFor5,
                goals_against_5 = @GoalsAgainst5,
                wins_5 = @Wins5,
                draws_5 = @Draws5,
                losses_5 = @Losses5,
                updated_at = @UpdatedAt
            WHERE team_id = @TeamId AND competition_id = @CompetitionId";

        internal const string SelectTeamSeasonStatsByTeamAndCompetition = @"
            SELECT 
                id AS Id,
                team_id AS TeamId,
                competition_id AS CompetitionId,
                goals_for_5 AS GoalsFor5,
                goals_against_5 AS GoalsAgainst5,
                wins_5 AS Wins5,
                draws_5 AS Draws5,
                losses_5 AS Losses5,
                updated_at AS UpdatedAt
            FROM team_season_stats
            WHERE team_id = @TeamId AND competition_id = @CompetitionId";

        internal const string SelectTeamSeasonStatsByCompetition = @"
            SELECT 
                id AS Id,
                team_id AS TeamId,
                competition_id AS CompetitionId,
                goals_for_5 AS GoalsFor5,
                goals_against_5 AS GoalsAgainst5,
                wins_5 AS Wins5,
                draws_5 AS Draws5,
                losses_5 AS Losses5,
                updated_at AS UpdatedAt
            FROM team_season_stats
            WHERE competition_id = @CompetitionId";

        internal const string SelectTeamSeasonStatsOlderThan = @"
            SELECT 
                id AS Id,
                team_id AS TeamId,
                competition_id AS CompetitionId,
                goals_for_5 AS GoalsFor5,
                goals_against_5 AS GoalsAgainst5,
                wins_5 AS Wins5,
                draws_5 AS Draws5,
                losses_5 AS Losses5,
                updated_at AS UpdatedAt
            FROM team_season_stats
            WHERE updated_at < @Threshold";

        internal const string DeleteTeamSeasonStatsByTeamAndCompetition = 
            "DELETE FROM team_season_stats WHERE team_id = @TeamId AND competition_id = @CompetitionId";

        internal const string DeleteTeamSeasonStatsByCompetition = 
            "DELETE FROM team_season_stats WHERE competition_id = @CompetitionId";
    }
}
