namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Queries;

internal static class CountriesQueries
{
    internal const string SelectAllCountries = @"
        SELECT CountryCode, CountryName
        FROM Countries
        ORDER BY CountryName";

    internal const string SelectCountryByCode = @"
        SELECT CountryCode, CountryName
        FROM Countries
        WHERE CountryCode = @CountryCode";
}
