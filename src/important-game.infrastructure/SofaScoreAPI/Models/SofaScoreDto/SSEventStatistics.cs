using System.Text.Json.Serialization;

namespace important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto
{
    public record SSEventStatistics(
        [property: JsonPropertyName("statistics")] IReadOnlyList<SSPeriodStatistic> Statistics
    );

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public record SSGroupStatistic(
        [property: JsonPropertyName("groupName")] string GroupName,
        [property: JsonPropertyName("statisticsItems")] IReadOnlyList<SSStatisticsItem> StatisticsItems
    );

    public record SSPeriodStatistic(
        [property: JsonPropertyName("period")] string Period,
        [property: JsonPropertyName("groups")] IReadOnlyList<SSGroupStatistic> Groups
    );

    public record SSStatisticsItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("home")] string Home,
        [property: JsonPropertyName("away")] string Away,
        [property: JsonPropertyName("compareCode")] int CompareCode,
        [property: JsonPropertyName("statisticsType")] string StatisticsType,
        [property: JsonPropertyName("valueType")] string ValueType,
        [property: JsonPropertyName("homeValue")] double HomeValue,
        [property: JsonPropertyName("awayValue")] double AwayValue,
        [property: JsonPropertyName("renderType")] int RenderType,
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("homeTotal")] int? HomeTotal,
        [property: JsonPropertyName("awayTotal")] int? AwayTotal
    );


}
