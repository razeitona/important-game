using System;
using FluentAssertions;
using important_game.infrastructure.ImportantMatch.Models.Processors;

namespace important_game.infrastructure.tests.ImportantMatch.Models;

public class EventStatusTests
{
    [Fact]
    public void GetGameTime_ReturnsNinetyPlusInjuryWhenPeriodMissing()
    {
        var status = new EventStatus
        {
            Period = null,
            InjuryTime1 = 2,
            InjuryTime2 = 1
        };

        status.GetGameTime().Should().Be(93);
    }

    [Fact]
    public void GetGameTime_PeriodOneReflectsElapsedMinutes()
    {
        var status = new EventStatus
        {
            Period = "period1",
            MatchStartTimestamp = (long)TimeSpan.FromMinutes(17).TotalSeconds
        };

        var expected = (int)(DateTime.UtcNow - TimeSpan.FromSeconds(status.MatchStartTimestamp)).TimeOfDay.TotalMinutes;
        status.GetGameTime().Should().BeInRange(expected - 1, expected + 1);
    }

    [Fact]
    public void GetGameTime_PeriodTwoAddsFirstHalfInjuryTime()
    {
        var status = new EventStatus
        {
            Period = "period2",
            InjuryTime1 = 4,
            MatchPeriodStartTimestamp = (long)TimeSpan.FromMinutes(12).TotalSeconds
        };

        var expected = 45 + status.InjuryTime1!.Value + (int)(DateTime.UtcNow - TimeSpan.FromSeconds(status.MatchPeriodStartTimestamp)).TimeOfDay.TotalMinutes;
        status.GetGameTime().Should().BeInRange(expected - 1, expected + 1);
    }

    [Fact]
    public void GetGameTime_ReturnsZeroForUnknownPeriod()
    {
        var status = new EventStatus
        {
            Period = "penalties"
        };

        status.GetGameTime().Should().Be(0);
    }
}
