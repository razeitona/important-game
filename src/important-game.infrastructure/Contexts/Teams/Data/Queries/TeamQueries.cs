namespace important_game.infrastructure.Contexts.Teams.Data.Queries;

/// <summary>
/// SQL queries for Team repository operations.
/// </summary>
internal static class TeamQueries
{
    internal const string SelectAllTeams = "SELECT * FROM Teams";

    internal const string CheckTeamExists = "SELECT COUNT(*) FROM Teams WHERE Id = @Id";

    internal const string InsertTeam = @"
            INSERT INTO Teams (Name, ShortName, ThreeLetterName, NormalizedName)
            VALUES (@Name, @ShortName, @ThreeLetterName, @NormalizedName);
            SELECT last_insert_rowid();";

    internal const string OldInsertTeam = @"
            INSERT INTO teams (id, name)
            VALUES (@Id, @Name)";

    internal const string SelectTeamById = @"
            SELECT 
                id AS Id,
                name AS Name
            FROM teams 
            WHERE id = @Id";

    internal const string SelectTeamsByIds = @"
            SELECT 
                id AS Id,
                name AS Name
            FROM teams 
            WHERE id IN @Ids
            ORDER BY id";
}
