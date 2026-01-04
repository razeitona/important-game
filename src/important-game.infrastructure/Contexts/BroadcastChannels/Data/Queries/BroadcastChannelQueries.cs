namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Queries;

internal static class BroadcastChannelQueries
{
    internal const string CheckIfBroadcastExists = @"
        SELECT ChannelId
        FROM BroadcastChannels
        WHERE Code = @Code AND CountryCode = @CountryCode;";

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
        SELECT ChannelId, Name, Code, CountryCode, IsActive, CreatedAt
        FROM BroadcastChannels
        WHERE CountryCode = @CountryCode AND IsActive = 1
        ORDER BY Name";

    internal const string SelectChannelsByIds = @"
        SELECT ChannelId, Name, Code, IsActive, CreatedAt
        FROM BroadcastChannels
        WHERE ChannelId IN @ChannelIds AND IsActive = 1
        ORDER BY Name";

    internal const string SelectCountriesForChannel = @"
        SELECT c.CountryCode, c.CountryName
        FROM Countries c
        INNER JOIN BroadcastChannels bc ON c.CountryCode = bc.CountryCode
        WHERE bc.ChannelId = @ChannelId
        ORDER BY c.CountryName";

    internal const string UpdateBroadcastChannel = @"
        UPDATE BroadcastChannels
        SET Name = @Name,
            Code = @Code,
            CountryCode = @CountryCode
        WHERE ChannelId = @ChannelId;";

    internal const string InsertBroadcastChannel = @"
        INSERT INTO BroadcastChannels (Name, Code, CountryCode, IsActive, CreatedAt)
        VALUES (@Name, @Code, @CountryCode, 1, datetime('now'));
        SELECT last_insert_rowid();";
}
