using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    public class SSPlayerSeason
    {
        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("stats")]
        public SSPlayerStats Stats { get; set; }
    }
}
