namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for Rivalry repository operations.
    /// </summary>
    internal static class RivalryQueries
    {
        internal const string CheckRivalryExists = "SELECT COUNT(*) FROM rivalry WHERE id = @Id";

        internal const string UpdateRivalry = @"
            UPDATE rivalry 
            SET rivalry_value = @RivarlyValue
            WHERE id = @Id";

        internal const string InsertRivalry = @"
            INSERT INTO rivalry (team_one_id, team_two_id, rivalry_value)
            VALUES (@TeamOneId, @TeamTwoId, @RivarlyValue)";

        internal const string SelectRivalryByTeamId = @"
            SELECT 
                id AS Id,
                team_one_id AS TeamOneId,
                team_two_id AS TeamTwoId,
                rivalry_value AS RivarlyValue
            FROM rivalry 
            WHERE (team_one_id = @TeamOneId AND team_two_id = @TeamTwoId)
               OR (team_two_id = @TeamOneId AND team_one_id = @TeamTwoId)
            LIMIT 1";
    }
}
