using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Models;

public class TwitterPostResponse
{
    [JsonPropertyName("data")]
    public TwitterPostData? Data { get; set; }
}

public class TwitterPostData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
