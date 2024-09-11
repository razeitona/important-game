using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public record SSDailyUniqueTournament(
        [property: JsonPropertyName("date")] string Date,
        [property: JsonPropertyName("uniqueTournamentIds")] IReadOnlyList<int> UniqueTournamentIds
    );

    public record SSSportTournaments(
        [property: JsonPropertyName("dailyUniqueTournaments")] IReadOnlyList<SSDailyUniqueTournament> DailyUniqueTournaments
    );





}
