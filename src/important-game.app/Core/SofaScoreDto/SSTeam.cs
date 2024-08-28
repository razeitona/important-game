using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{

    public record SSTeam
    (
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("shortName")] string ShortName,
        [property: JsonPropertyName("teamColors")] SSTeamColors TeamColors
    );


    public record SSTeamColors
    (
        [property: JsonPropertyName("primary")] string Primary,
        [property: JsonPropertyName("secondary")] string Secondary,
        [property: JsonPropertyName("text")] string Text
    );
}
