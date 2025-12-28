using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto
{
    public record SSHeadToHead(
        [property: JsonPropertyName("events")] IReadOnlyList<SSEvent> Events
    );

}
