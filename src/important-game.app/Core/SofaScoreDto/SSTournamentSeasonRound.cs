using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace important_game.ui.Core.SofaScoreDto
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public record SSCurrentRound(
        [property: JsonPropertyName("round")] int Round
    );

    public record SSTournamentSeasonRound(
        [property: JsonPropertyName("currentRound")] SSCurrentRound CurrentRound,
        [property: JsonPropertyName("rounds")] IReadOnlyList<SSRound> Rounds
    );

    public record SSRound(
        [property: JsonPropertyName("round")] int Round
    );

}
