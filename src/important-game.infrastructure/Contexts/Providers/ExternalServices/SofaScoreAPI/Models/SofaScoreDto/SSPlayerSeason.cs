using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto
{
    public class SSPlayerSeason
    {
        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("stats")]
        public SSPlayerStats Stats { get; set; }
    }
}
