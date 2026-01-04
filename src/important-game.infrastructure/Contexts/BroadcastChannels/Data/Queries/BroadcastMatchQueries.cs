namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Queries;

internal static class BroadcastMatchQueries
{
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
        INNER JOIN Countries c ON bc.CountryCode = c.CountryCode
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
        INNER JOIN Countries c ON bc.CountryCode = c.CountryCode
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
        INNER JOIN Countries c ON bc.CountryCode = c.CountryCode
        WHERE mb.ChannelId = @ChannelId AND bc.IsActive = 1
        ORDER BY mb.MatchId;";

    internal const string SelectBroadcastCountriesForMatch = @"
        SELECT DISTINCT c.CountryCode, c.CountryName
        FROM MatchBroadcasts mb
        INNER JOIN BroadcastChannels bc ON mb.ChannelId = bc.ChannelId
        INNER JOIN Countries c ON bc.CountryCode = c.CountryCode
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

    internal const string UpsertBroadcastMatches = @"
        INSERT INTO MatchBroadcasts (MatchId, ChannelId, CreatedAt) 
        VALUES (@MatchId, @ChannelId, datetime('now'))
        ON CONFLICT(MatchId, ChannelId) DO UPDATE SET
            MatchId = excluded.MatchId,
            ChannelId = excluded.ChannelId;";

    internal const string DeletePasthMatchBroadcasts = @"
        DELETE FROM MatchBroadcasts WHERE MatchId IN (
            SELECT MatchId FROM Matches WHERE MatchDateUTC < @CurrentUtcDate
        );";
}
