using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto
{
    public record SSTournament
    (
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("uniqueTournament")] SSUniqueTournament UniqueTournament,
        [property: JsonPropertyName("priority")] int? Priority,
        [property: JsonPropertyName("competitionType")] int? CompetitionType,
        [property: JsonPropertyName("isGroup")] bool? IsGroup
    );

    public record SSUniqueTournament
    (
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("titleHolder")] TournamentTitleHolder TitleHolder,
        [property: JsonPropertyName("primaryColorHex")] string PrimaryColor,
        [property: JsonPropertyName("secondaryColorHex")] string SecondaryColor,
        [property: JsonPropertyName("hasPerformanceGraphFeature")] bool HasPerformanceGraphFeature,
        [property: JsonPropertyName("hasEventPlayerStatistics")] bool HasEventPlayerStatistics,
        [property: JsonPropertyName("displayInverseHomeAwayTeams")] bool DisplayInverseHomeAwayTeams
    );

    public record TournamentTitleHolder
    (
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name
    );

}
