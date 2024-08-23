using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    public record SSTournamentTable(
        [property: JsonPropertyName("standings")] IReadOnlyList<SSTeamStanding> Standings
    );

    public record SSTableRow(
        [property: JsonPropertyName("team")] SSTeam Team,
        [property: JsonPropertyName("descriptions")] IReadOnlyList<object> Descriptions,
        [property: JsonPropertyName("position")] int? Position,
        [property: JsonPropertyName("matches")] int? Matches,
        [property: JsonPropertyName("wins")] int? Wins,
        [property: JsonPropertyName("scoresFor")] int? ScoresFor,
        [property: JsonPropertyName("scoresAgainst")] int? ScoresAgainst,
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("losses")] int? Losses,
        [property: JsonPropertyName("draws")] int? Draws,
        [property: JsonPropertyName("points")] int? Points
    );

    public record SSTeamStanding(
        [property: JsonPropertyName("tournament")] SSTournament Tournament,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("descriptions")] IReadOnlyList<object> Descriptions,
        [property: JsonPropertyName("rows")] IReadOnlyList<SSTableRow> Rows,
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("updatedAtTimestamp")] int? UpdatedAtTimestamp
    );

}
