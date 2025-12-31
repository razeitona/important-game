namespace important_game.infrastructure.Contexts.Teams.Data.Queries;

/// <summary>
/// SQL queries for Team repository operations.
/// </summary>
internal static class TeamQueries
{
    internal const string SelectAllTeams = "SELECT * FROM Teams";

    internal const string CheckTeamExists = "SELECT COUNT(*) FROM Teams WHERE Id = @Id";

    internal const string InsertTeam = @"
            INSERT INTO Teams (Name, ShortName, ThreeLetterName, NormalizedName, SlugName)
            VALUES (@Name, @ShortName, @ThreeLetterName, @NormalizedName, @SlugName);
            SELECT last_insert_rowid();";

    internal const string UpdateTeam = @"
            UPDATE Teams SET 
                Name = @Name, 
                ShortName = @ShortName, 
                ThreeLetterName = @ThreeLetterName, 
                NormalizedName = @NormalizedName, 
                SlugName = @SlugName
            WHERE 
                Id = @Id;";

    internal const string SelectTeamById = @"
            SELECT 
                *
            FROM Teams 
            WHERE id = @Id";

    internal const string SelectTeamsByIds = @"
            SELECT 
                *
            FROM Teams 
            WHERE id IN @Ids
            ORDER BY id";
}
