using System.Text.Json.Serialization;

namespace important_game.ui.Core.SofaScoreDto
{
    public record SSTournamentEvents(
        [property: JsonPropertyName("events")] List<SSEvent> Events,
        [property: JsonPropertyName("hasNextPage")] bool? HasNextPage
    );
    public record SSEvent(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("detailId")] int? DetailId,
        [property: JsonPropertyName("customId")] string CustomId,
        //[property: JsonPropertyName("tournament")] SSTournament Tournament,
        [property: JsonPropertyName("season")] SSSeason Season,
        [property: JsonPropertyName("winnerCode")] int? WinnerCode,
        [property: JsonPropertyName("roundInfo")] SSEventRoundInfo RoundInfo,
        [property: JsonPropertyName("status")] SSEventStatus Status,
        [property: JsonPropertyName("homeTeam")] SSTeam HomeTeam,
        [property: JsonPropertyName("homeScore")] SSEventScore HomeScore,
        [property: JsonPropertyName("awayTeam")] SSTeam AwayTeam,
        [property: JsonPropertyName("awayScore")] SSEventScore AwayScore,
        [property: JsonPropertyName("startTimestamp")] long StartTimestamp,
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("time")] SSEventTime Time,
        [property: JsonPropertyName("finalResultOnly")] bool? FinalResultOnly,
        [property: JsonPropertyName("hasGlobalHighlights")] bool? HasGlobalHighlights,
        [property: JsonPropertyName("hasXg")] bool? HasXg,
        [property: JsonPropertyName("hasEventPlayerStatistics")] bool? HasEventPlayerStatistics,
        [property: JsonPropertyName("hasEventPlayerHeatMap")] bool? HasEventPlayerHeatMap
    );
    public record SSEventRoundInfo(
        [property: JsonPropertyName("round")] int? Round
    );

    public record SSEventStatus(
        [property: JsonPropertyName("code")] int? Code,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("type")] string Type
    );


    public record SSEventTime(
        [property: JsonPropertyName("injuryTime1")] int? InjuryTime1,
        [property: JsonPropertyName("injuryTime2")] int? InjuryTime2,
        [property: JsonPropertyName("currentPeriodStartTimestamp")] int? CurrentPeriodStartTimestamp
    );

    public record SSEventScore(
    [property: JsonPropertyName("current")] int Current,
    [property: JsonPropertyName("display")] int? Display,
    [property: JsonPropertyName("period1")] int? Period1,
    [property: JsonPropertyName("period2")] int? Period2,
    [property: JsonPropertyName("normaltime")] int? Normaltime
);

}
