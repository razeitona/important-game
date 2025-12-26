using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using MatchEntity = important_game.infrastructure.Contexts.Matches.Data.Entities.Match;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models.Processors;
using important_game.infrastructure.LeagueProcessors;
using Moq;

namespace important_game.infrastructure.tests.ImportantMatch.Live;

public class ExcitmentMatchLiveProcessorTests
{
    [Fact]
    public async Task ProcessLiveMatchData_WhenStatisticsMissing_MarksMatchFinished()
    {
        var match = CreateMatch();
        match.MatchStatus = MatchStatus.Live;

        var leagueProcessor = new Mock<ILeagueProcessor>();
        leagueProcessor.Setup(p => p.GetEventStatisticsAsync(match.Id.ToString()))
            .ReturnsAsync((EventStatistics?)null);

        var processor = new ExcitmentMatchLiveProcessor(leagueProcessor.Object);

        var result = await processor.ProcessLiveMatchData(match);

        result.Should().BeNull();
        match.MatchStatus.Should().Be(MatchStatus.Finished);
        leagueProcessor.Verify(p => p.GetEventInformationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessLiveMatchData_WhenEventInfoMissing_DoesNotAlterExistingStatus()
    {
        var match = CreateMatch();
        match.MatchStatus = MatchStatus.Upcoming;

        var statistics = BuildStatistics(out _, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);

        var leagueProcessor = new Mock<ILeagueProcessor>();
        leagueProcessor.Setup(p => p.GetEventStatisticsAsync(match.Id.ToString()))
            .ReturnsAsync(statistics);
        leagueProcessor.Setup(p => p.GetEventInformationAsync(match.Id.ToString()))
            .ReturnsAsync((EventInfo)null!);

        var processor = new ExcitmentMatchLiveProcessor(leagueProcessor.Object);

        var result = await processor.ProcessLiveMatchData(match);

        result.Should().BeNull();
        match.MatchStatus.Should().Be(MatchStatus.Upcoming);
        leagueProcessor.Verify(p => p.GetEventInformationAsync(match.Id.ToString()), Times.Once);
    }

    [Fact]
    public async Task ProcessLiveMatchData_ComputesLiveScoresAndUpdatesMatch()
    {
        var match = CreateMatch();
        match.ExcitmentScore = 0.55;
        match.MatchStatus = MatchStatus.Upcoming;

        var statistics = BuildStatistics(
            out var homeShots,
            out var awayShots,
            out var homeShotsOnTarget,
            out var awayShotsOnTarget,
            out var xGoalsHome,
            out var xGoalsAway,
            out var foulsHome,
            out var foulsAway,
            out var yellowHome,
            out var yellowAway,
            out var redHome,
            out var redAway,
            out var possessionHome,
            out var possessionAway);

        var status = new EventStatus
        {
            Period = null,
            MatchStartTimestamp = 0,
            MatchPeriodStartTimestamp = 0,
            InjuryTime1 = 3,
            InjuryTime2 = 0,
            StatusCode = EventMatchStatus.SecondHalf
        };

        var eventInfo = new EventInfo
        {
            Id = match.Id,
            HomeTeam = new TeamInfo { Id = match.HomeTeamId, Name = "Home" },
            AwayTeam = new TeamInfo { Id = match.AwayTeamId, Name = "Away" },
            HomeTeamScore = 2,
            AwayTeamScore = 1,
            Status = status
        };

        var leagueProcessor = new Mock<ILeagueProcessor>();
        leagueProcessor.Setup(p => p.GetEventStatisticsAsync(match.Id.ToString()))
            .ReturnsAsync(statistics);
        leagueProcessor.Setup(p => p.GetEventInformationAsync(match.Id.ToString()))
            .ReturnsAsync(eventInfo);

        var processor = new ExcitmentMatchLiveProcessor(leagueProcessor.Object);

        var result = await processor.ProcessLiveMatchData(match);

        result.Should().NotBeNull();

        var gameTime = status.GetGameTime();
        var expected = CalculateExpectedScores(
            match.ExcitmentScore,
            eventInfo,
            gameTime,
            homeShots,
            awayShots,
            homeShotsOnTarget,
            awayShotsOnTarget,
            xGoalsHome,
            xGoalsAway,
            foulsHome,
            foulsAway,
            yellowHome,
            yellowAway,
            redHome,
            redAway,
            possessionHome,
            possessionAway);

        var liveMatch = result!;
        liveMatch.ExcitmentScore.Should().BeApproximately(expected.LiveExcitementScore, 1e-10);
        liveMatch.ScoreLineScore.Should().BeApproximately(expected.ScoreLineScore, 1e-10);
        liveMatch.XGoalsScore.Should().BeApproximately(expected.XGoalsScore, 1e-10);
        liveMatch.TotalFoulsScore.Should().BeApproximately(expected.TotalFoulsScore, 1e-10);
        liveMatch.TotalCardsScore.Should().BeApproximately(expected.TotalCardsScore, 1e-10);
        liveMatch.PossesionScore.Should().BeApproximately(expected.PossessionScore, 1e-10);
        liveMatch.BigChancesScore.Should().BeApproximately(expected.BigChancesScore, 1e-10);
        liveMatch.Minutes.Should().Be(gameTime);

        match.MatchStatus.Should().Be(MatchStatus.Live);
        match.HomeScore.Should().Be(eventInfo.HomeTeamScore);
        match.AwayScore.Should().Be(eventInfo.AwayTeamScore);
        match.UpdatedDateUTC.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));

        leagueProcessor.Verify(p => p.GetEventStatisticsAsync(match.Id.ToString()), Times.Once);
        leagueProcessor.Verify(p => p.GetEventInformationAsync(match.Id.ToString()), Times.Once);
    }

    private static MatchEntity CreateMatch()
    {
        var competition = new CompetitionEntity
        {
            CompetitionId = 10,
            Name = "League",
            PrimaryColor = "#111111",
            BackgroundColor = "#222222",
            LeagueRanking = 0.5,
            IsActive = true
        };

        var homeTeam = new Team { Id = 1, Name = "Home FC" };
        var awayTeam = new Team { Id = 2, Name = "Away FC" };

        return new MatchEntity
        {
            Id = 999,
            CompetitionId = competition.CompetitionId,
            Competition = competition,
            MatchDateUTC = DateTime.UtcNow,
            MatchStatus = MatchStatus.Upcoming,
            HomeTeamId = homeTeam.Id,
            HomeTeam = homeTeam,
            AwayTeamId = awayTeam.Id,
            AwayTeam = awayTeam,
            ExcitmentScore = 0.5,
            UpdatedDateUTC = DateTime.UtcNow.AddHours(-1)
        };
    }

    private static EventStatistics BuildStatistics(
        out double homeShots,
        out double awayShots,
        out double homeShotsOnTarget,
        out double awayShotsOnTarget,
        out double xGoalsHome,
        out double xGoalsAway,
        out double foulsHome,
        out double foulsAway,
        out double yellowHome,
        out double yellowAway,
        out double redHome,
        out double redAway,
        out double possessionHome,
        out double possessionAway)
    {
        const string period = "ALL";
        const string group = "Match overview";

        homeShots = 10;
        awayShots = 8;
        homeShotsOnTarget = 6;
        awayShotsOnTarget = 5;
        xGoalsHome = 1.6;
        xGoalsAway = 0.9;
        foulsHome = 12;
        foulsAway = 8;
        yellowHome = 2;
        yellowAway = 1;
        redHome = 1;
        redAway = 0;
        possessionHome = 42;
        possessionAway = 70;

        var statistics = new EventStatistics();
        statistics.Statistics[period] = new Dictionary<string, Dictionary<string, StatisticsItem>>
        {
            [group] = new Dictionary<string, StatisticsItem>()
        };

        var overview = statistics.Statistics[period][group];
        overview["totalShotsOnGoal"] = CreateStat(homeShots, awayShots);
        overview["shotsOnGoal"] = CreateStat(homeShotsOnTarget, awayShotsOnTarget);
        overview["expectedGoals"] = CreateStat(xGoalsHome, xGoalsAway);
        overview["fouls"] = CreateStat(foulsHome, foulsAway);
        overview["yellowCards"] = CreateStat(yellowHome, yellowAway);
        overview["redCards"] = CreateStat(redHome, redAway);
        overview["ballPossession"] = CreateStat(possessionHome, possessionAway);

        return statistics;
    }

    private static StatisticsItem CreateStat(double home, double away) =>
        new("key", "name", string.Empty, home, null, string.Empty, away, null, 0);

    private static ExpectedScores CalculateExpectedScores(
        double baseExcitement,
        EventInfo eventInfo,
        double gameTime,
        double homeShots,
        double awayShots,
        double homeShotsOnTarget,
        double awayShotsOnTarget,
        double xGoalsHome,
        double xGoalsAway,
        double foulsHome,
        double foulsAway,
        double yellowHome,
        double yellowAway,
        double redHome,
        double redAway,
        double possessionHome,
        double possessionAway)
    {
        const double scoreLineCoef = 0.2d;
        const double xGoalsCoef = 0.2d;
        const double foulsCoef = 0.15d;
        const double cardsCoef = 0.15d;
        const double possessionCoef = 0.15d;
        const double bigChanceCoef = 0.15d;

        var scoreLineDiff = Math.Abs(eventInfo.HomeTeamScore - eventInfo.AwayTeamScore);
        var scoreLineData = 1d - (scoreLineDiff / 4d);
        var scoreLineValue = scoreLineData * (1d + (gameTime / 90d));

        var totalShots = homeShots + awayShots;
        var totalShotsPerGameTime = totalShots / Math.Max(1, gameTime);
        var shotsTimeValue = Math.Min(1.0, totalShotsPerGameTime / 8d);

        var shotsOnTarget = homeShotsOnTarget + awayShotsOnTarget;
        var shotsOnTargetRatio = totalShots > 0 ? shotsOnTarget / totalShots : 0;

        var totalXGoals = xGoalsHome + xGoalsAway;
        var xGoalsValue = (totalXGoals / 3d) * (1d + (gameTime / 90d));

        var totalFouls = foulsHome + foulsAway;
        var totalFoulsExp = 1d - Math.Min(1.0, totalFouls / 25d);
        var totalFoulsValue = totalFoulsExp * (1d + (gameTime / 90d));

        var yellowTotal = yellowHome + yellowAway;
        var redTotal = redHome + redAway;
        var homeRedBoost = (eventInfo.HomeTeamScore > eventInfo.AwayTeamScore && redHome > redAway) ? 0.15d * redHome : 0d;
        var awayRedBoost = (eventInfo.AwayTeamScore > eventInfo.HomeTeamScore && redAway > redHome) ? 0.15d * redAway : 0d;
        var redCardBoost = (redTotal * 0.2d) + homeRedBoost + awayRedBoost;
        var cardsValue = redCardBoost + (1d - ((yellowTotal + redTotal) / 8d));
        var totalCardsValue = cardsValue * (1d + (gameTime / 90d));

        double losingTeamPossession;
        if (eventInfo.HomeTeamScore > eventInfo.AwayTeamScore)
        {
            losingTeamPossession = possessionAway;
        }
        else if (eventInfo.HomeTeamScore < eventInfo.AwayTeamScore)
        {
            losingTeamPossession = possessionHome;
        }
        else
        {
            losingTeamPossession = Math.Min(possessionHome, possessionAway);
        }

        double losingTeamBonus = 0d;
        if (losingTeamPossession > 65d)
        {
            losingTeamBonus = 0.4d;
        }
        else if (losingTeamPossession > 60d)
        {
            losingTeamBonus = 0.25d;
        }
        else if (losingTeamPossession > 55d)
        {
            losingTeamBonus = 0.15d;
        }

        var possessionDiff = Math.Abs(possessionHome - possessionAway) / 200d;
        var possessionTeam = 1d - possessionDiff;
        var possessionValue = (possessionTeam * (1d + (gameTime / 90d))) + losingTeamBonus;

        var bigChancesTotal = (shotsOnTargetRatio * totalShots) / 2d + totalXGoals;
        var bigChancesValue = (bigChancesTotal / 8d) * (1d + (gameTime / 90d));

        var componentScores = new[]
        {
            scoreLineValue * scoreLineCoef,
            xGoalsValue * xGoalsCoef,
            totalFoulsValue * foulsCoef,
            totalCardsValue * cardsCoef,
            possessionValue * possessionCoef,
            bigChancesValue * bigChanceCoef
        };

        var weightedSum = componentScores.Sum();
        var normalizedScore = weightedSum + (shotsTimeValue * 0.1);
        var excitementDelta = ((normalizedScore * 0.4d) + 0.1d);
        var liveExcitement = baseExcitement + excitementDelta;

        return new ExpectedScores(
            Math.Min(1.0, liveExcitement),
            scoreLineValue,
            xGoalsValue,
            totalFoulsValue,
            totalCardsValue,
            possessionValue,
            bigChancesValue);
    }

    private sealed record ExpectedScores(
        double LiveExcitementScore,
        double ScoreLineScore,
        double XGoalsScore,
        double TotalFoulsScore,
        double TotalCardsScore,
        double PossessionScore,
        double BigChancesScore);
}




