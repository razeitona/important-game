using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    public class SSTournamentSeasons
    {
        [JsonPropertyName("seasons")]
        public List<SSSeason> Seasons { get; set; }
    }

    public class SSSeason
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

    }
}
