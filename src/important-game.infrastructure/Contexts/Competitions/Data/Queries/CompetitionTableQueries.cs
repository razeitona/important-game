namespace important_game.infrastructure.Contexts.Competitions.Data.Queries;

/// <summary>
/// SQL queries for CompetitionTable repository operations.
/// </summary>
internal static class CompetitionTableQueries
{

    internal const string CheckCompetitionTableExists = @"
        SELECT COUNT(*) 
        FROM CompetitionTable 
        WHERE CompetitionId = @CompetitionId 
            AND SeasonId = @SeasonId 
            AND TeamId = @TeamId";

    internal const string UpsertCompetitionTable = @"
        INSERT INTO CompetitionTable (
            CompetitionId, SeasonId, TeamId, Position, Points, Matches, Wins, Draws, Losses,
            GoalsFor, GoalsAgainst
        ) VALUES (
            @CompetitionId, @SeasonId, @TeamId, @Position, @Points, @Matches, @Wins, @Draws, @Losses,
            @GoalsFor, @GoalsAgainst
        )
        ON CONFLICT(CompetitionId, SeasonId, TeamId) DO UPDATE SET
            Position = excluded.Position,
            Points = excluded.Points,
            Matches = excluded.Matches,
            Wins = excluded.Wins,
            Draws = excluded.Draws,
            Losses = excluded.Losses,
            GoalsFor = excluded.GoalsFor,
            GoalsAgainst = excluded.GoalsAgainst;";

    internal const string InsertCompetitionTable = @"
            INSERT INTO CompetitionTable (
                CompetitionId, SeasonId, TeamId, Position, Points, matches, Wins, Draws, Losses,
                GoalsFor, GoalsAgainst
            ) VALUES (
                @CompetitionId, @SeasonId, @TeamId, @Position, @Points, @Matches, @Wins, @Draws, @Losses,
                @GoalsFor, @GoalsAgainst
            )";

    internal const string UpdateCompetitionTable = @"
            UPDATE CompetitionTable 
            SET Position = @Position,
                Points = @Points,
                matches = @Matches,
                Wins = @Wins,
                Draws = @Draws,
                Losses = @Losses,
                GoalsFor = @GoalsFor,
                GoalsAgainst = @GoalsAgainst
            WHERE CompetitionId = @CompetitionId AND SeasonId = @SeasonId AND TeamId = @TeamId";

    internal const string SelectCompetitionTableByCompetitionAndSeason = @"
            SELECT 
                CompetitionId,
                SeasonId,
                TeamId,
                Position,
                Points,
                matches,
                Wins,
                Draws,
                Losses,
                GoalsFor,
                GoalsAgainst
            FROM CompetitionTable
            WHERE CompetitionId = @CompetitionId AND SeasonId = @SeasonId
            ORDER BY Position ASC";

    internal const string DeleteCompetitionTableByCompetitionAndSeason =
        "DELETE FROM CompetitionTable WHERE CompetitionId = @CompetitionId AND SeasonId = @SeasonId";

    internal const string SelectCompetitionTableByTeamCompetitionAndSeason = @"
            SELECT 
                CompetitionId,
                SeasonId,
                TeamId,
                Position,
                Points,
                matches,
                Wins,
                Draws,
                Losses,
                GoalsFor,
                GoalsAgainst
            FROM CompetitionTable
            WHERE CompetitionId = @CompetitionId AND SeasonId = @SeasonId AND TeamId = @TeamId";
}
