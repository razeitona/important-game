using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    public class SSTournament
    {
        [JsonPropertyName("uniqueTournament")]
        public UniqueTournament UniqueTournament { get; set; }
    }

    public class UniqueTournament
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("titleHolder")]
        public TournamentTitleHolder TitleHolder { get; set; }

    }

    public class TournamentTitleHolder
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
