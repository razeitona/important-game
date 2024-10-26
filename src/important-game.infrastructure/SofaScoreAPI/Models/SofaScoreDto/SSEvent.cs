using System.Text.Json.Serialization;

namespace important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public record SSTeamScore(
        [property: JsonPropertyName("current")] int Current,
        [property: JsonPropertyName("display")] int? Display,
        [property: JsonPropertyName("period1")] int? Period1,
        [property: JsonPropertyName("period2")] int? Period2,
        [property: JsonPropertyName("normaltime")] int? Normaltime
    );

    public record SSEvent(
        [property: JsonPropertyName("tournament")] SSTournament Tournament,
        [property: JsonPropertyName("season")] SSSeason Season,
        [property: JsonPropertyName("roundInfo")] SSRound RoundInfo,
        [property: JsonPropertyName("customId")] string CustomId,
        [property: JsonPropertyName("status")] SSEventStatus Status,
        [property: JsonPropertyName("homeTeam")] SSTeam HomeTeam,
        [property: JsonPropertyName("awayTeam")] SSTeam AwayTeam,
        [property: JsonPropertyName("homeScore")] SSTeamScore HomeScore,
        [property: JsonPropertyName("awayScore")] SSTeamScore AwayScore,
        [property: JsonPropertyName("time")] SSEventTime Time,
        [property: JsonPropertyName("hasGlobalHighlights")] bool? HasGlobalHighlights,
        [property: JsonPropertyName("detailId")] int? DetailId,
        [property: JsonPropertyName("crowdsourcingDataDisplayEnabled")] bool? CrowdsourcingDataDisplayEnabled,
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("defaultPeriodCount")] int? DefaultPeriodCount,
        [property: JsonPropertyName("defaultPeriodLength")] int? DefaultPeriodLength,
        [property: JsonPropertyName("defaultOvertimeLength")] int? DefaultOvertimeLength,
        [property: JsonPropertyName("statusTime")] SSEventStatusTime StatusTime,
        [property: JsonPropertyName("crowdsourcingEnabled")] bool? CrowdsourcingEnabled,
        [property: JsonPropertyName("currentPeriodStartTimestamp")] int? CurrentPeriodStartTimestamp,
        [property: JsonPropertyName("startTimestamp")] int StartTimestamp,
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("lastPeriod")] string? LastPeriod,
        [property: JsonPropertyName("finalResultOnly")] bool? FinalResultOnly,
        [property: JsonPropertyName("feedLocked")] bool? FeedLocked,
        [property: JsonPropertyName("fanRatingEvent")] bool? FanRatingEvent,
        [property: JsonPropertyName("showTotoPromo")] bool? ShowTotoPromo,
        [property: JsonPropertyName("isEditor")] bool? IsEditor
    );


    public record SSEventInfo(
        [property: JsonPropertyName("event")] SSEvent EventData
    );


    public record SSEventStadium(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("capacity")] int? Capacity
    );

    public record SSEventStatusTime(
        [property: JsonPropertyName("prefix")] string Prefix,
        [property: JsonPropertyName("initial")] int? Initial,
        [property: JsonPropertyName("max")] int? Max,
        [property: JsonPropertyName("timestamp")] int? Timestamp,
        [property: JsonPropertyName("extra")] int? Extra
    );


}
