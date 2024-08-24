using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{


    public record SSTeam
   (
        //public int Id { get; set; }
        //public string Slug { get; set; }
        //public string ShortName { get; set; }
        //public string Name { get; set; }
        //public List<SSPlayer> Players { get; set; }

        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("shortName")] string ShortName

  );
}
