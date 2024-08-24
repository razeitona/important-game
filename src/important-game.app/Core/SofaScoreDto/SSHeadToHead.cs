using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    public record SSHeadToHead(
        [property: JsonPropertyName("events")] IReadOnlyList<SSEvent> Events
    );

}
