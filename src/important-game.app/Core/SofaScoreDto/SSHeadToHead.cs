using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    //public record ManagerDuel(
    //    [property: JsonPropertyName("homeWins")] int? HomeWins,
    //    [property: JsonPropertyName("awayWins")] int? AwayWins,
    //    [property: JsonPropertyName("draws")] int? Draws
    //);

    public record SSHeadToHead(
        [property: JsonPropertyName("teamDuel")] SSTeamDuel TeamDuel
    //[property: JsonPropertyName("managerDuel")] ManagerDuel ManagerDuel
    );

    public record SSTeamDuel(
        [property: JsonPropertyName("homeWins")] int? HomeWins,
        [property: JsonPropertyName("awayWins")] int? AwayWins,
        [property: JsonPropertyName("draws")] int? Draws
    );


}
