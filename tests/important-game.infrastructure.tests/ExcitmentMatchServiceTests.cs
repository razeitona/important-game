using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.Data.Repositories;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models;
using Moq;
using HeadToHeadEntity = important_game.infrastructure.Contexts.Matches.Data.Entities.Headtohead;
using MatchEntity = important_game.infrastructure.Contexts.Matches.Data.Entities.Match;
using important_game.infrastructure.Contexts.Matches.Data;

namespace important_game.infrastructure.tests.ImportantMatch;

public class ExcitmentMatchServiceTests
{
    private static ExcitmentMatchService CreateService(
        out Mock<IMatchRepository> repository,
        out Mock<ILiveMatchRepository> liveRepository,
        out Mock<IExcitmentMatchProcessor> processor,
        out Mock<IExcitmentMatchLiveProcessor> liveProcessor)
    {
        repository = new Mock<IMatchRepository>(MockBehavior.Strict);
        liveRepository = new Mock<ILiveMatchRepository>(MockBehavior.Strict);
        processor = new Mock<IExcitmentMatchProcessor>(MockBehavior.Strict);
        liveProcessor = new Mock<IExcitmentMatchLiveProcessor>(MockBehavior.Strict);

        return new ExcitmentMatchService(repository.Object, liveRepository.Object, processor.Object, liveProcessor.Object);
    }

    [Fact]
    public async Task CalculateUpcomingMatchsExcitment_ShouldDelegateToProcessor()
    {
        // Arrange
        var service = CreateService(out var repository, out var liveRepository, out var processor, out var liveProcessor);
        processor.Setup(p => p.CalculateUpcomingMatchsExcitment()).Returns(Task.CompletedTask).Verifiable();

        // Act
        await service.CalculateUpcomingMatchsExcitment();

        // Assert
        processor.Verify();
        repository.VerifyNoOtherCalls();
        liveRepository.VerifyNoOtherCalls();
        liveProcessor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CalculateUnfinishedMatchExcitment_PersistsLiveUpdatesAndSkipsFutureFixtures()
    {
        // Arrange
        var service = CreateService(out var repository, out var liveRepository, out var processor, out var liveProcessor);

        var pastMatch = CreateMatch(1, DateTime.UtcNow.AddMinutes(-45));
        var futureMatch = CreateMatch(2, DateTime.UtcNow.AddHours(2));

        repository.Setup(r => r.GetUnfinishedMatchesAsync())
            .ReturnsAsync(new List<MatchEntity> { pastMatch, futureMatch });

        repository.Setup(r => r.SaveMatchAsync(pastMatch)).Returns(Task.CompletedTask).Verifiable();
        liveRepository.Setup(r => r.SaveLiveMatchAsync(It.IsAny<LiveMatch>())).Returns(Task.CompletedTask).Verifiable();

        liveProcessor.Setup(lp => lp.ProcessLiveMatchData(pastMatch))
            .ReturnsAsync(new LiveMatch { MatchId = pastMatch.Id, ExcitmentScore = 0.9, Minutes = 65 });

        // Act
        await service.CalculateUnfinishedMatchExcitment();

        // Assert
        repository.Verify(r => r.GetUnfinishedMatchesAsync(), Times.Once);
        repository.Verify(r => r.SaveMatchAsync(pastMatch), Times.Once);
        repository.Verify(r => r.SaveMatchAsync(futureMatch), Times.Never);
        liveRepository.Verify(r => r.SaveLiveMatchAsync(It.Is<LiveMatch>(lm => lm.MatchId == pastMatch.Id && lm.ExcitmentScore == 0.9)), Times.Once);
        repository.VerifyNoOtherCalls();
        liveRepository.VerifyNoOtherCalls();

        liveProcessor.Verify(lp => lp.ProcessLiveMatchData(pastMatch), Times.Once);
        liveProcessor.Verify(lp => lp.ProcessLiveMatchData(futureMatch), Times.Never);
        liveProcessor.VerifyNoOtherCalls();
        processor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAllMatchesAsync_MapsEntitiesAndOrdersByKickoff()
    {
        // Arrange
        var service = CreateService(out var repository, out var liveRepository, out var processor, out var liveProcessor);

        var laterMatch = CreateMatch(10, DateTime.UtcNow.AddHours(6), status: MatchStatus.Live, excitmentScore: 0.72, liveScores: new[] { 0.65, 0.8 });
        var earlierMatch = CreateMatch(11, DateTime.UtcNow.AddHours(1), status: MatchStatus.Upcoming, excitmentScore: 0.55);

        repository.Setup(r => r.GetUpcomingMatchesAsync())
            .ReturnsAsync(new List<MatchEntity> { laterMatch, earlierMatch });

        // Act
        var results = await service.GetAllMatchesAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Select(r => r.Id).Should().ContainInOrder(earlierMatch.Id, laterMatch.Id);

        var first = results.First();
        first.IsLive.Should().BeFalse();
        first.LiveExcitementScore.Should().BeApproximately(earlierMatch.ExcitmentScore, 1e-10);
        first.HomeTeam.Slug.Should().Be($"home-team{first.Id}");
        first.AwayTeam.Slug.Should().Be($"away-team{first.Id}");

        var second = results.Last();
        second.IsLive.Should().BeTrue();
        second.LiveExcitementScore.Should().BeApproximately(0.8, 1e-10);
        second.League.Name.Should().Be(laterMatch.Competition.Name);

        repository.Verify(r => r.GetUpcomingMatchesAsync(), Times.Once);
        repository.VerifyNoOtherCalls();
        liveRepository.VerifyNoOtherCalls();
        processor.VerifyNoOtherCalls();
        liveProcessor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLiveMatchesAsync_MapsLatestLiveSnapshot()
    {
        // Arrange
        var service = CreateService(out var repository, out var liveRepository, out var processor, out var liveProcessor);

        var matchWithLive = CreateMatch(20, DateTime.UtcNow.AddMinutes(-15), status: MatchStatus.Live, excitmentScore: 0.66, liveScores: new[] { 0.4, 0.7 }, liveMinutes: new[] { 15d, 55d });
        var matchWithoutLive = CreateMatch(21, DateTime.UtcNow.AddMinutes(-30), status: MatchStatus.Live, excitmentScore: 0.45, includeLive: false);

        repository.Setup(r => r.GetUnfinishedMatchesAsync())
            .ReturnsAsync(new List<MatchEntity> { matchWithLive, matchWithoutLive });

        // Act
        var results = await service.GetLiveMatchesAsync();

        // Assert
        results.Should().HaveCount(2);
        var first = results.Single(r => r.Id == matchWithLive.Id);
        first.LiveExcitementScore.Should().BeApproximately(0.7, 1e-10);
        first.Minutes.Should().Be(55);
        first.IsLive.Should().BeTrue();

        var second = results.Single(r => r.Id == matchWithoutLive.Id);
        second.LiveExcitementScore.Should().BeApproximately(0, 1e-10);
        second.Minutes.Should().Be(0);

        repository.Verify(r => r.GetUnfinishedMatchesAsync(), Times.Once);
        repository.VerifyNoOtherCalls();
        liveRepository.VerifyNoOtherCalls();
        processor.VerifyNoOtherCalls();
        liveProcessor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMatchByIdAsync_ReturnsNullWhenMatchMissing()
    {
        // Arrange
        var service = CreateService(out var repository, out var liveRepository, out var processor, out var liveProcessor);
        repository.Setup(r => r.GetMatchByIdAsync(999)).ReturnsAsync((MatchEntity?)null);

        // Act
        var result = await service.GetMatchByIdAsync(999);

        // Assert
        result.Should().BeNull();
        repository.Verify(r => r.GetMatchByIdAsync(999), Times.Once);
        repository.VerifyNoOtherCalls();
        liveRepository.VerifyNoOtherCalls();
        processor.VerifyNoOtherCalls();
        liveProcessor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMatchByIdAsync_ProvidesDetailedBreakdown()
    {
        // Arrange
        var service = CreateService(out var repository, out var liveRepository, out var processor, out var liveProcessor);

        var match = CreateMatch(
            id: 42,
            kickoff: DateTime.UtcNow.AddHours(-1),
            status: MatchStatus.Live,
            excitmentScore: 0.71,
            homeForm: "1,1,0,-1",
            awayForm: "0,-1,1,1",
            competitionScore: 0.7,
            fixtureScore: 0.5,
            formScore: 0.8,
            goalsScore: 0.65,
            standingScore: 0.6,
            h2hScore: 0.55,
            rivalryScore: 0.3,
            titleHolderScore: 1.0,
            liveScores: new[] { 0.6 },
            liveMinutes: new[] { 75d });

        repository.Setup(r => r.GetMatchByIdAsync(match.Id)).ReturnsAsync(match);

        // Act
        var dto = await service.GetMatchByIdAsync(match.Id);

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(match.Id);
        dto.HomeTeam.Form.Should().ContainInOrder(MatchResultType.Win, MatchResultType.Win, MatchResultType.Draw, MatchResultType.Lost);
        dto.HomeTeam.IsTitleHolder.Should().BeTrue();
        dto.AwayTeam.Form.Should().ContainInOrder(MatchResultType.Draw, MatchResultType.Lost, MatchResultType.Win, MatchResultType.Win);
        dto.AwayTeam.IsTitleHolder.Should().BeFalse();
        dto.Headtohead.Should().HaveCount(2);
        dto.ExcitmentScoreDetail.Should().ContainKeys("Score Line", "xGoals", "Fouls", "Cards", "Possession", "Big chances");
        dto.ExcitmentScoreDetail["Score Line"].Show.Should().BeTrue();
        dto.Description.Should().Contain("This match has");
        dto.Description.Should().Contain("tight match");
        dto.Description.Should().Contain("few stoppages");
        dto.LiveExcitementScore.Should().BeApproximately(0.6, 1e-10);
        dto.Minutes.Should().BeApproximately(75, 1e-10);

        repository.Verify(r => r.GetMatchByIdAsync(match.Id), Times.Once);
        repository.VerifyNoOtherCalls();
        liveRepository.VerifyNoOtherCalls();
        processor.VerifyNoOtherCalls();
        liveProcessor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMatchByIdAsync_ReturnsPreMatchBreakdown()
    {
        // Arrange
        var service = CreateService(out var repository, out var liveRepository, out var processor, out var liveProcessor);

        var match = CreateMatch(
            id: 77,
            kickoff: DateTime.UtcNow.AddDays(2),
            status: MatchStatus.Upcoming,
            excitmentScore: 0.64,
            competitionScore: 0.85,
            fixtureScore: 0.7,
            formScore: 0.75,
            goalsScore: 0.68,
            standingScore: 0.62,
            h2hScore: 0.58,
            rivalryScore: 0.9,
            titleHolderScore: 1d,
            includeLive: false);

        repository.Setup(r => r.GetMatchByIdAsync(match.Id)).ReturnsAsync(match);

        // Act
        var dto = await service.GetMatchByIdAsync(match.Id);

        // Assert
        dto.Should().NotBeNull();
        dto!.ExcitmentScoreDetail.Should().ContainKeys("League Coeficient", "League Standings", "Fixture Importance", "Teams Form", "Teams Goals", "Head to head", "Rivalry", "Title Holder");
        dto.ExcitmentScoreDetail["Rivalry"].Show.Should().BeFalse();
        dto.ExcitmentScoreDetail["League Coeficient"].Value.Should().BeApproximately(0.85, 1e-10);
        dto.Description.Should().Contain("This match has");
        dto.Description.Should().Contain("significant impact on league standings");
        dto.LiveExcitementScore.Should().BeApproximately(0, 1e-10);
        dto.Minutes.Should().Be(0);

        repository.Verify(r => r.GetMatchByIdAsync(match.Id), Times.Once);
        repository.VerifyNoOtherCalls();
        liveRepository.VerifyNoOtherCalls();
        processor.VerifyNoOtherCalls();
        liveProcessor.VerifyNoOtherCalls();
    }

    private static MatchEntity CreateMatch(
        int id,
        DateTime kickoff,
        MatchStatus status = MatchStatus.Upcoming,
        double excitmentScore = 0.5,
        string homeForm = "1,1,1,0,0",
        string awayForm = "1,0,1,0,0",
        double competitionScore = 0.75,
        double fixtureScore = 0.5,
        double formScore = 0.6,
        double goalsScore = 0.6,
        double standingScore = 0.55,
        double h2hScore = 0.5,
        double rivalryScore = 0.2,
        double titleHolderScore = 1d,
        bool includeLive = true,
        double[]? liveScores = null,
        double[]? liveMinutes = null)
    {
        var competition = new CompetitionEntity
        {
            CompetitionId = 100 + id,
            Name = $"League {id}",
            BackgroundColor = "#123456",
            PrimaryColor = "#654321",
            LeagueRanking = competitionScore,
            IsActive = true,
            TitleHolderTeamId = 10 + id
        };

        var homeTeam = new Team { Id = 10 + id, Name = $"Home Team{id}" };
        var awayTeam = new Team { Id = 20 + id, Name = $"Away Team{id}" };

        var match = new MatchEntity
        {
            Id = id,
            MatchDateUTC = kickoff,
            CompetitionId = competition.CompetitionId,
            Competition = competition,
            MatchStatus = status,
            HomeTeamId = homeTeam.Id,
            AwayTeamId = awayTeam.Id,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeForm = homeForm,
            AwayForm = awayForm,
            HomeTeamPosition = 1,
            AwayTeamPosition = 2,
            ExcitmentScore = excitmentScore,
            CompetitionScore = competitionScore,
            FixtureScore = fixtureScore,
            FormScore = formScore,
            GoalsScore = goalsScore,
            CompetitionStandingScore = standingScore,
            HeadToHeadScore = h2hScore,
            RivalryScore = rivalryScore,
            TitleHolderScore = titleHolderScore,
            UpdatedDateUTC = DateTime.UtcNow.AddMinutes(-10)
        };

        match.HeadToHead = new List<HeadToHeadEntity>
        {
            new()
            {
                MatchId = id,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                MatchDateUTC = kickoff.AddDays(-7),
                HomeTeamScore = 2,
                AwayTeamScore = 1,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam
            },
            new()
            {
                MatchId = id,
                HomeTeamId = awayTeam.Id,
                AwayTeamId = homeTeam.Id,
                MatchDateUTC = kickoff.AddDays(-30),
                HomeTeamScore = 0,
                AwayTeamScore = 3,
                HomeTeam = awayTeam,
                AwayTeam = homeTeam
            }
        };

        if (includeLive)
        {
            liveScores ??= Array.Empty<double>();
            liveMinutes ??= Array.Empty<double>();
            match.LiveMatches = liveScores.Select((score, index) => new LiveMatch
            {
                MatchId = id,
                ExcitmentScore = score,
                Minutes = (int)(liveMinutes.Length > index ? liveMinutes[index] : 0),
                RegisteredDate = kickoff.AddMinutes(index * 5)
            }).ToList();
        }
        else
        {
            match.LiveMatches = new List<LiveMatch>();
        }

        return match;
    }
}


