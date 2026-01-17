using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Models;

public class TwitterUserResponse
{
    [JsonPropertyName("data")]
    public TwitterUserData? Data { get; set; }
}

public class TwitterUserData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}
