namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Queries;

internal static class BroadcastChannelQueries
{
    // ==================== Countries Queries ====================

    internal const string SelectAllCountries = @"
        SELECT CountryCode, CountryName
        FROM Countries
        ORDER BY CountryName";

    internal const string SelectCountryByCode = @"
        SELECT CountryCode, CountryName
        FROM Countries
        WHERE CountryCode = @CountryCode";

    // ==================== BroadcastChannels Queries ====================

    internal const string SelectAllActiveChannels = @"
        SELECT ChannelId, Name, Code, IsActive, CreatedAt
        FROM BroadcastChannels
        WHERE IsActive = 1
        ORDER BY Name";

    internal const string SelectChannelById = @"
        SELECT ChannelId, Name, Code, IsActive, CreatedAt
        FROM BroadcastChannels
        WHERE ChannelId = @ChannelId";

    internal const string SelectChannelsByCountryCode = @"
        SELECT bc.ChannelId, bc.Name, bc.Code, bc.IsActive, bc.CreatedAt
        FROM BroadcastChannels bc
        INNER JOIN BroadcastChannelCountries bcc ON bc.ChannelId = bcc.ChannelId
        WHERE bcc.CountryCode = @CountryCode AND bc.IsActive = 1
        ORDER BY bc.Name";

    internal const string SelectChannelsByIds = @"
        SELECT ChannelId, Name, Code, IsActive, CreatedAt
        FROM BroadcastChannels
        WHERE ChannelId IN @ChannelIds AND IsActive = 1
        ORDER BY Name";

    internal const string SelectCountriesForChannel = @"
        SELECT c.CountryCode, c.CountryName
        FROM Countries c
        INNER JOIN BroadcastChannelCountries bcc ON c.CountryCode = bcc.CountryCode
        WHERE bcc.ChannelId = @ChannelId
        ORDER BY c.CountryName";

    // ==================== MatchBroadcasts Queries ====================

    internal const string SelectBroadcastsByMatchId = @"
        SELECT
            mb.MatchId,
            mb.ChannelId,
            bc.Name as ChannelName,
            bc.Code as ChannelCode,
            c.CountryCode,
            c.CountryName
        FROM MatchBroadcasts mb
        INNER JOIN BroadcastChannels bc ON mb.ChannelId = bc.ChannelId
        INNER JOIN BroadcastChannelCountries bcc ON mb.ChannelId = bcc.ChannelId
        INNER JOIN Countries c ON bcc.CountryCode = c.CountryCode
        WHERE mb.MatchId = @MatchId AND bc.IsActive = 1
        ORDER BY c.CountryName, bc.Name;";

    internal const string SelectBroadcastsByMatchIds = @"
        SELECT
            mb.MatchId,
            mb.ChannelId,
            bc.Name as ChannelName,
            bc.Code as ChannelCode,
            c.CountryCode,
            c.CountryName
        FROM MatchBroadcasts mb
        INNER JOIN BroadcastChannels bc ON mb.ChannelId = bc.ChannelId
        INNER JOIN BroadcastChannelCountries bcc ON mb.ChannelId = bcc.ChannelId
        INNER JOIN Countries c ON bcc.CountryCode = c.CountryCode
        WHERE mb.MatchId IN @MatchIds AND bc.IsActive = 1
        ORDER BY mb.MatchId, c.CountryName, bc.Name;";

    internal const string SelectBroadcastsByChannelId = @"
        SELECT
            mb.MatchId,
            mb.ChannelId,
            bc.Name as ChannelName,
            bc.Code as ChannelCode,
            c.CountryCode,
            c.CountryName
        FROM MatchBroadcasts mb
        INNER JOIN BroadcastChannels bc ON mb.ChannelId = bc.ChannelId
        INNER JOIN BroadcastChannelCountries bcc ON mb.ChannelId = bcc.ChannelId
        INNER JOIN Countries c ON bcc.CountryCode = c.CountryCode
        WHERE mb.ChannelId = @ChannelId AND bc.IsActive = 1
        ORDER BY mb.MatchId;";

    internal const string SelectBroadcastCountriesForMatch = @"
        SELECT DISTINCT c.CountryCode, c.CountryName
        FROM MatchBroadcasts mb
        INNER JOIN BroadcastChannels bc ON mb.ChannelId = bc.ChannelId
        INNER JOIN BroadcastChannelCountries bcc ON mb.ChannelId = bcc.ChannelId
        INNER JOIN Countries c ON bcc.CountryCode = c.CountryCode
        WHERE mb.MatchId = @MatchId
        ORDER BY c.CountryName";

    internal const string InsertMatchBroadcast = @"
        INSERT INTO MatchBroadcasts (MatchId, ChannelId, CreatedAt)
        VALUES (@MatchId, @ChannelId, @CreatedAt)";

    internal const string DeleteMatchBroadcast = @"
        DELETE FROM MatchBroadcasts
        WHERE MatchId = @MatchId AND ChannelId = @ChannelId";

    internal const string DeleteMatchBroadcastsByMatchId = @"
        DELETE FROM MatchBroadcasts
        WHERE MatchId = @MatchId";

    // ==================== User Favorite Channels Queries ====================

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
            bcc.CountryCode
        FROM BroadcastChannels bc
        INNER JOIN UserFavoriteBroadcastChannels ufbc ON bc.ChannelId = ufbc.ChannelId
        INNER JOIN BroadcastChannelCountries bcc ON bc.ChannelId = bcc.ChannelId
        WHERE ufbc.UserId = @UserId AND bc.IsActive = 1
        ORDER BY bcc.CountryCode, bc.Name";

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

    // ==================== TV Listings Queries ====================

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
