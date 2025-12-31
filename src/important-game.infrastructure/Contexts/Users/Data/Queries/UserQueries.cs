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

    // Match Votes Queries
    public const string GetUserVote = @"
        SELECT UserId, MatchId, VoteType, VotedAt
        FROM MatchVotes
        WHERE UserId = @UserId AND MatchId = @MatchId";

    public const string AddOrUpdateVote = @"
        INSERT INTO MatchVotes (UserId, MatchId, VoteType, VotedAt)
        VALUES (@UserId, @MatchId, @VoteType, @VotedAt)
        ON CONFLICT(UserId, MatchId)
        DO UPDATE SET VoteType = @VoteType, VotedAt = @VotedAt";

    public const string RemoveVote = @"
        DELETE FROM MatchVotes
        WHERE UserId = @UserId AND MatchId = @MatchId";

    public const string GetMatchVoteCount = @"
        SELECT COUNT(*)
        FROM MatchVotes
        WHERE MatchId = @MatchId";

    public const string GetUserVotesForMatches = @"
        SELECT UserId, MatchId, VoteType, VotedAt
        FROM MatchVotes
        WHERE UserId = @UserId AND MatchId IN @MatchIds";
}
