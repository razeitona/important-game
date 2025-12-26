namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for Team repository operations.
    /// </summary>
    internal static class TeamQueries
    {
        internal const string CheckTeamExists = "SELECT COUNT(*) FROM team WHERE id = @Id";

        internal const string InsertTeam = @"
            INSERT INTO team (id, name)
            VALUES (@Id, @Name)";

        internal const string SelectTeamById = @"
            SELECT 
                id AS Id,
                name AS Name
            FROM team 
            WHERE id = @Id";

        internal const string SelectTeamsByIds = @"
            SELECT 
                id AS Id,
                name AS Name
            FROM team 
            WHERE id IN @Ids
            ORDER BY id";
    }
}
