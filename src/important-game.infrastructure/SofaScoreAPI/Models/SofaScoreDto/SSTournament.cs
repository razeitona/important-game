using System.Text.Json.Serialization;

namespace important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto
{
    public record SSTournament
    (
        [property: JsonPropertyName("uniqueTournament")] UniqueTournament UniqueTournament
    );

    public record UniqueTournament
    (
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("titleHolder")] TournamentTitleHolder TitleHolder,
        [property: JsonPropertyName("primaryColorHex")] string PrimaryColor,
        [property: JsonPropertyName("secondaryColorHex")] string SecondaryColor
    );

    public record TournamentTitleHolder
    (
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name
    );

}
