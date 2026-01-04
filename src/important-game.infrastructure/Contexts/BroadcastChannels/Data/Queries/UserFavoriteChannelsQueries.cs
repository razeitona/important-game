namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Queries;

internal static class UserFavoriteChannelsQueries
{

    internal const string SelectUserFavoriteChannelIds = @"
        SELECT ChannelId
        FROM UserFavoriteBroadcastChannels
        WHERE UserId = @UserId
        ORDER BY AddedAt DESC";

    internal const string SelectUserFavoriteChannels = @"
        SELECT
            bc.ChannelId,
            bc.Name,
            bc.Code,
            bc.IsActive,
            bc.CreatedAt
        FROM BroadcastChannels bc
        INNER JOIN UserFavoriteBroadcastChannels ufbc ON bc.ChannelId = ufbc.ChannelId
        WHERE ufbc.UserId = @UserId AND bc.IsActive = 1
        ORDER BY bc.Name";

    internal const string SelectUserFavoriteChannelsByCountry = @"
        SELECT
            bc.ChannelId,
            bc.Name,
            bc.Code,
            bc.IsActive,
            bc.CreatedAt,
            bc.CountryCode
        FROM BroadcastChannels bc
        INNER JOIN UserFavoriteBroadcastChannels ufbc ON bc.ChannelId = ufbc.ChannelId
        WHERE ufbc.UserId = @UserId AND bc.IsActive = 1
        ORDER BY bc.CountryCode, bc.Name";

    internal const string CheckIsFavoriteChannel = @"
        SELECT COUNT(1)
        FROM UserFavoriteBroadcastChannels
        WHERE UserId = @UserId AND ChannelId = @ChannelId";

    internal const string InsertUserFavoriteChannel = @"
        INSERT INTO UserFavoriteBroadcastChannels (UserId, ChannelId, AddedAt)
        VALUES (@UserId, @ChannelId, @AddedAt)";

    internal const string DeleteUserFavoriteChannel = @"
        DELETE FROM UserFavoriteBroadcastChannels
        WHERE UserId = @UserId AND ChannelId = @ChannelId";

    internal const string SelectUpcomingMatchesWithUserFavoriteChannels = @"
        SELECT DISTINCT
            m.MatchId,
            m.CompetitionId,
            m.MatchDateUTC,
            m.HomeTeamId,
            m.AwayTeamId,
            m.HomeScore,
            m.AwayScore,
            m.ExcitmentScore,
            m.IsFinished,
            mb.ChannelId,
            bc.Name as ChannelName,
            bc.Code as ChannelCode,
            mb.CountryCode,
            c.CountryName
        FROM Matches m
        INNER JOIN MatchBroadcasts mb ON m.MatchId = mb.MatchId
        INNER JOIN BroadcastChannels bc ON mb.ChannelId = bc.ChannelId
        INNER JOIN Countries c ON mb.CountryCode = c.CountryCode
        INNER JOIN UserFavoriteBroadcastChannels ufbc ON bc.ChannelId = ufbc.ChannelId
        WHERE ufbc.UserId = @UserId
          AND m.IsFinished = 0
          AND bc.IsActive = 1
          AND datetime(m.MatchDateUTC) >= datetime(@StartDate)
          AND datetime(m.MatchDateUTC) < datetime(@EndDate)
        ORDER BY m.MatchDateUTC, c.CountryName, bc.Name";
}
