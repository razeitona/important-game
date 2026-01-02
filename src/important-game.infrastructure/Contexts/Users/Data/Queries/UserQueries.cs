namespace important_game.infrastructure.Contexts.Users.Data.Queries;

public static class UserQueries
{
    public const string GetUserByGoogleId = @"
        SELECT UserId, GoogleId, Email, Name, ProfilePictureUrl, PreferredTimezone, CreatedAt, LastLoginAt
        FROM Users
        WHERE GoogleId = @GoogleId";

    public const string GetUserById = @"
        SELECT UserId, GoogleId, Email, Name, ProfilePictureUrl, PreferredTimezone, CreatedAt, LastLoginAt
        FROM Users
        WHERE UserId = @UserId";

    public const string CreateUser = @"
        INSERT INTO Users (GoogleId, Email, Name, ProfilePictureUrl, PreferredTimezone, CreatedAt, LastLoginAt)
        VALUES (@GoogleId, @Email, @Name, @ProfilePictureUrl, @PreferredTimezone, @CreatedAt, @LastLoginAt);
        SELECT last_insert_rowid();";

    public const string UpdateLastLogin = @"
        UPDATE Users
        SET LastLoginAt = @LastLoginAt
        WHERE UserId = @UserId";

    public const string UpdateUserPreferences = @"
        UPDATE Users
        SET PreferredTimezone = @PreferredTimezone,
            Name = @Name,
            ProfilePictureUrl = @ProfilePictureUrl
        WHERE UserId = @UserId";

    public const string DeleteUser = @"
        DELETE FROM Users
        WHERE UserId = @UserId";

    // Favorite Matches Queries
    public const string GetUserFavoriteMatches = @"
        SELECT ufm.UserId, ufm.MatchId, ufm.AddedAt
        FROM UserFavoriteMatches ufm
        WHERE ufm.UserId = @UserId
        ORDER BY ufm.AddedAt DESC";

    public const string AddFavoriteMatch = @"
        INSERT OR IGNORE INTO UserFavoriteMatches (UserId, MatchId, AddedAt)
        VALUES (@UserId, @MatchId, @AddedAt)";

    public const string RemoveFavoriteMatch = @"
        DELETE FROM UserFavoriteMatches
        WHERE UserId = @UserId AND MatchId = @MatchId";

    public const string IsFavoriteMatch = @"
        SELECT COUNT(1)
        FROM UserFavoriteMatches
        WHERE UserId = @UserId AND MatchId = @MatchId";

    public const string GetFavoriteMatchCount = @"
        SELECT COUNT(1)
        FROM UserFavoriteMatches
        WHERE MatchId = @MatchId";

    // Favorite Teams Queries
    public const string GetUserFavoriteTeamIds = @"
        SELECT TeamId
        FROM UserFavoriteTeams
        WHERE UserId = @UserId
        ORDER BY AddedAt DESC";

    public const string AddFavoriteTeam = @"
        INSERT OR IGNORE INTO UserFavoriteTeams (UserId, TeamId, AddedAt)
        VALUES (@UserId, @TeamId, @AddedAt)";

    public const string RemoveFavoriteTeam = @"
        DELETE FROM UserFavoriteTeams
        WHERE UserId = @UserId AND TeamId = @TeamId";

    public const string IsFavoriteTeam = @"
        SELECT COUNT(1)
        FROM UserFavoriteTeams
        WHERE UserId = @UserId AND TeamId = @TeamId";

    public const string GetMatchesFromFavoriteTeams = @"
        SELECT DISTINCT m.*,
               c.CompetitionId,
               c.Name as 'CompetitionName',
               c.PrimaryColor as 'CompetitionPrimaryColor',
               c.BackgroundColor as 'CompetitionBackgroundColor',
               ht.Name as 'HomeTeamName',
               at.Name as 'AwayTeamName'
        FROM Matches m
        INNER JOIN UserFavoriteTeams uft ON (m.HomeTeamId = uft.TeamId OR m.AwayTeamId = uft.TeamId)
        INNER JOIN Competitions c ON m.CompetitionId = c.CompetitionId
        INNER JOIN Teams ht ON m.HomeTeamId = ht.Id
        INNER JOIN Teams at ON m.AwayTeamId = at.Id
        WHERE uft.UserId = @UserId
          AND m.IsFinished = 0
          AND datetime(m.MatchDateUTC) >= datetime('now')
        ORDER BY m.MatchDateUTC ASC";
}
