using System.Collections.Generic;
using FluentAssertions;
using important_game.infrastructure.ImportantMatch.Models.Processors;

namespace important_game.infrastructure.tests.ImportantMatch.Models;

public class EventStatisticsTests
{
    [Fact]
    public void GetHomeStatValue_ReturnsTrueWhenStatExists()
    {
        var stats = new EventStatistics();
        stats.Statistics["ALL"] = new Dictionary<string, Dictionary<string, StatisticsItem>>
        {
            ["Match overview"] = new Dictionary<string, StatisticsItem>
            {
                ["expectedGoals"] = new("expectedGoals", "Expected Goals", string.Empty, 1.4, null, string.Empty, 1.0, null, 0)
            }
        };

        stats.GetHomeStatValue("ALL", "Match overview", "expectedGoals", out var value).Should().BeTrue();
        value.Should().Be(1.4);
    }

    [Fact]
    public void GetHomeStatValue_ReturnsFalseWhenStatMissing()
    {
        var stats = new EventStatistics();

        stats.GetHomeStatValue("ALL", "Match overview", "shotsOnGoal", out var value).Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void GetAwayStatValue_ReturnsFalseWhenGroupMissing()
    {
        var stats = new EventStatistics();
        stats.Statistics["FIRST"] = new();

        stats.GetAwayStatValue("FIRST", "Match overview", "fouls", out _).Should().BeFalse();
    }
}
