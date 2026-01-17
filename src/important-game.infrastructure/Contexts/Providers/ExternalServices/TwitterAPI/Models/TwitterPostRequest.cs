using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Models;

public class TwitterPostRequest
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}
