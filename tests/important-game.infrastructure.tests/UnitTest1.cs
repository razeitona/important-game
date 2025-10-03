using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Data;
using important_game.infrastructure.ImportantMatch.Data.Entities;
using important_game.infrastructure.ImportantMatch.Models;
using important_game.infrastructure.ImportantMatch.Models.Processors;
using important_game.infrastructure.LeagueProcessors;
using important_game.infrastructure.Telegram;
using Moq;
using HeadToHeadEntity = important_game.infrastructure.ImportantMatch.Data.Entities.Headtohead;
using MatchEntity = important_game.infrastructure.ImportantMatch.Data.Entities.Match;

namespace important_game.infrastructure.tests.ImportantMatch;

public class ExcitementMatchProcessorTests
{
    private const double CompetitionCoef = 0.15d;
    private const double FixtureCoef = 0.10d;
    private const double TeamFormCoef = 0.10d;
    private const double TeamGoalsCoef = 0.15d;
    private const double TableRankCoef = 0.15d;
    private const double HeadToHeadCoef = 0.10d;
    private const double TitleHolderCoef = 0.10d;
    private const double RivalryCoef = 0.15d;
    private const double NumericTolerance = 1e-10;
    private const double LateCompetitionCoef = 0.05d;
    private const double LateFixtureCoef = 0.25d;
    private const double LateTeamFormCoef = 0.05d;
    private const double LateTeamGoalsCoef = 0.2d;
    private const double LateTableRankCoef = 0.33d;
    private const double LateHeadToHeadCoef = 0.05d;
    private const double LateTitleHolderCoef = 0.05d;
    private const double LateRivalryCoef = 0.02d;

    [Fact]
    public async Task CalculateUpcomingMatchsExcitment_ComputesScoresAndPersistsMatch()
    {
        // Arrange
        var scenario = CreateScenario();

        var leagueProcessor = new Mock<ILeagueProcessor>();
        leagueProcessor.Setup(p => p.GetLeagueDataAsync(scenario.League.Id)).ReturnsAsync(scenario.League);
        leagueProcessor.Setup(p => p.GetUpcomingMatchesAsync(scenario.League.Id, scenario.League.CurrentSeason.Id))
            .ReturnsAsync(new LeagueUpcomingFixtures { scenario.Fixture });
        leagueProcessor.Setup(p => p.GetLeagueTableAsync(scenario.League.Id, scenario.League.CurrentSeason.Id))
            .ReturnsAsync(scenario.LeagueStanding);

        var matchRepository = new Mock<IExctimentMatchRepository>();
        matchRepository.Setup(r => r.GetActiveCompetitionsAsync())
            .ReturnsAsync(new List<Competition> { scenario.Competition });
        matchRepository.Setup(r => r.GetCompetitionActiveMatchesAsync(scenario.Competition.Id))
            .ReturnsAsync(new List<MatchEntity>());
        matchRepository.Setup(r => r.GetRivalryByTeamIdAsync(scenario.HomeTeam.Id, scenario.AwayTeam.Id))
            .ReturnsAsync(scenario.Rivalry);
        matchRepository.Setup(r => r.SaveTeamAsync(It.IsAny<Team>()))
            .ReturnsAsync((Team team) => team);

        MatchEntity? capturedMatch = null;
        matchRepository.Setup(r => r.SaveMatchAsync(It.IsAny<MatchEntity>()))
            .Callback<MatchEntity>(match => capturedMatch = match)
            .Returns(Task.CompletedTask);

        List<HeadToHeadEntity>? capturedHeadToHead = null;
        matchRepository.Setup(r => r.SaveHeadToHeadMatchesAsync(It.IsAny<List<HeadToHeadEntity>>()))
            .Callback<List<HeadToHeadEntity>>(list => capturedHeadToHead = list)
            .Returns(Task.CompletedTask);

        var processor = new ExcitementMatchProcessor(leagueProcessor.Object, matchRepository.Object, Mock.Of<ITelegramBot>());

        // Act
        await processor.CalculateUpcomingMatchsExcitment();

        // Assert
        capturedMatch.Should().NotBeNull();
        capturedMatch!.Id.Should().Be(scenario.Fixture.Id);
        capturedMatch.CompetitionId.Should().Be(scenario.League.Id);
        capturedMatch.HomeTeamId.Should().Be(scenario.HomeTeam.Id);
        capturedMatch.AwayTeamId.Should().Be(scenario.AwayTeam.Id);
        capturedMatch.HomeTeamPosition.Should().Be(1);
        capturedMatch.AwayTeamPosition.Should().Be(2);
        capturedMatch.MatchStatus.Should().Be(MatchStatus.Upcoming);
        capturedMatch.HomeForm.Should().Be("1,1,1,0,0");
        capturedMatch.AwayForm.Should().Be("1,0,1,0,0");

        var expectedCompetitionScore = scenario.Competition.LeagueRanking;
        var expectedFixtureScore = ComputeFixtureScoreBase(scenario.LeagueStanding);
        var expectedFormScore = ComputeFormScoreBase(scenario.HomeTeam.LastFixtures, scenario.AwayTeam.LastFixtures);
        var expectedGoalsScore = ComputeGoalsScoreBase(scenario.HomeTeam.LastFixtures, scenario.AwayTeam.LastFixtures);
        var expectedTableScore = ComputeTableScoreBase(scenario.HomeTeam, scenario.AwayTeam, scenario.LeagueStanding);
        var expectedHeadToHeadScore = ComputeHeadToHeadScoreBase(scenario.Fixture.HeadToHead, scenario.HomeTeam.Id, scenario.AwayTeam.Id);
        var expectedTitleHolderScore = ComputeTitleHolderScoreBase(scenario.League.TitleHolder, scenario.HomeTeam.Id, scenario.AwayTeam.Id);
        var expectedRivalryScore = scenario.Rivalry.RivarlyValue;

        capturedMatch.CompetitionScore.Should().BeApproximately(expectedCompetitionScore, NumericTolerance);
        capturedMatch.FixtureScore.Should().BeApproximately(expectedFixtureScore, NumericTolerance);
        capturedMatch.FormScore.Should().BeApproximately(expectedFormScore, NumericTolerance);
        capturedMatch.GoalsScore.Should().BeApproximately(expectedGoalsScore, NumericTolerance);
        capturedMatch.CompetitionStandingScore.Should().BeApproximately(expectedTableScore, NumericTolerance);
        capturedMatch.HeadToHeadScore.Should().BeApproximately(expectedHeadToHeadScore, NumericTolerance);
        capturedMatch.TitleHolderScore.Should().BeApproximately(expectedTitleHolderScore, NumericTolerance);
        capturedMatch.RivalryScore.Should().BeApproximately(expectedRivalryScore, NumericTolerance);

        var expectedScore =
            capturedMatch.CompetitionScore * CompetitionCoef +
            capturedMatch.FixtureScore * FixtureCoef +
            capturedMatch.FormScore * TeamFormCoef +
            capturedMatch.GoalsScore * TeamGoalsCoef +
            capturedMatch.CompetitionStandingScore * TableRankCoef +
            capturedMatch.HeadToHeadScore * HeadToHeadCoef +
            capturedMatch.TitleHolderScore * TitleHolderCoef +
            capturedMatch.RivalryScore * RivalryCoef;

        capturedMatch.ExcitmentScore.Should().BeApproximately(expectedScore, NumericTolerance);

        scenario.HomeTeam.Position.Should().Be(1);
        scenario.AwayTeam.Position.Should().Be(2);
        scenario.HomeTeam.IsTitleHolder.Should().BeTrue();
        scenario.AwayTeam.IsTitleHolder.Should().BeFalse();
        scenario.HomeTeam.H2hWins.Should().Be(2);
        scenario.AwayTeam.H2hWins.Should().Be(0);

        capturedHeadToHead.Should().NotBeNull();
        capturedHeadToHead!.Should().HaveCount(scenario.Fixture.HeadToHead.Count);
        capturedHeadToHead.Should().OnlyContain(h => h.MatchId == capturedMatch.Id);

        matchRepository.Verify(r => r.SaveMatchAsync(It.IsAny<MatchEntity>()), Times.Once);
        matchRepository.Verify(r => r.SaveHeadToHeadMatchesAsync(It.IsAny<List<HeadToHeadEntity>>()), Times.Once);
        matchRepository.Verify(r => r.SaveTeamAsync(It.IsAny<Team>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CalculateUpcomingMatchsExcitment_SkipsRecentlyUpdatedMatches()
    {
        // Arrange
        var scenario = CreateScenario();

        var leagueProcessor = new Mock<ILeagueProcessor>();
        leagueProcessor.Setup(p => p.GetLeagueDataAsync(scenario.League.Id)).ReturnsAsync(scenario.League);
        leagueProcessor.Setup(p => p.GetUpcomingMatchesAsync(scenario.League.Id, scenario.League.CurrentSeason.Id))
            .ReturnsAsync(new LeagueUpcomingFixtures { scenario.Fixture });
        leagueProcessor.Setup(p => p.GetLeagueTableAsync(scenario.League.Id, scenario.League.CurrentSeason.Id))
            .ReturnsAsync(scenario.LeagueStanding);

        var matchRepository = new Mock<IExctimentMatchRepository>();
        matchRepository.Setup(r => r.GetActiveCompetitionsAsync())
            .ReturnsAsync(new List<Competition> { scenario.Competition });
        matchRepository.Setup(r => r.GetCompetitionActiveMatchesAsync(scenario.Competition.Id))
            .ReturnsAsync(new List<MatchEntity>
            {
                new()
                {
                    Id = scenario.Fixture.Id,
                    UpdatedDateUTC = DateTime.UtcNow,
                    MatchDateUTC = DateTime.UtcNow,
                    CompetitionId = scenario.Competition.Id
                }
            });

        var processor = new ExcitementMatchProcessor(leagueProcessor.Object, matchRepository.Object, Mock.Of<ITelegramBot>());

        // Act
        await processor.CalculateUpcomingMatchsExcitment();

        // Assert
        matchRepository.Verify(r => r.SaveMatchAsync(It.IsAny<MatchEntity>()), Times.Never);
        matchRepository.Verify(r => r.SaveHeadToHeadMatchesAsync(It.IsAny<List<HeadToHeadEntity>>()), Times.Never);
        matchRepository.Verify(r => r.GetRivalryByTeamIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        matchRepository.Verify(r => r.SaveTeamAsync(It.IsAny<Team>()), Times.Never);
    }


    [Fact]
    public async Task CalculateUpcomingMatchsExcitment_SkipsWhenFixturesUnavailable()
    {
        var scenario = CreateScenario();

        var leagueProcessor = new Mock<ILeagueProcessor>();
        leagueProcessor.Setup(p => p.GetLeagueDataAsync(scenario.League.Id)).ReturnsAsync(scenario.League);
        leagueProcessor.Setup(p => p.GetUpcomingMatchesAsync(scenario.League.Id, scenario.League.CurrentSeason.Id))
            .ReturnsAsync((LeagueUpcomingFixtures?)null);

        var matchRepository = new Mock<IExctimentMatchRepository>();
        matchRepository.Setup(r => r.GetActiveCompetitionsAsync())
            .ReturnsAsync(new List<Competition> { scenario.Competition });
        matchRepository.Setup(r => r.GetCompetitionActiveMatchesAsync(scenario.Competition.Id))
            .ReturnsAsync(new List<MatchEntity>());

        var processor = new ExcitementMatchProcessor(leagueProcessor.Object, matchRepository.Object, Mock.Of<ITelegramBot>());

        await processor.CalculateUpcomingMatchsExcitment();

        leagueProcessor.Verify(p => p.GetLeagueTableAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        matchRepository.Verify(r => r.SaveMatchAsync(It.IsAny<MatchEntity>()), Times.Never);
        matchRepository.Verify(r => r.SaveHeadToHeadMatchesAsync(It.IsAny<List<HeadToHeadEntity>>()), Times.Never);
    }

    [Fact]
    public async Task CalculateUpcomingMatchsExcitment_UsesLateSeasonWeights()
    {
        var scenario = CreateScenario();
        scenario.LeagueStanding.CurrentRound = 32;
        scenario.LeagueStanding.TotalRounds = 36;

        var leagueProcessor = new Mock<ILeagueProcessor>();
        leagueProcessor.Setup(p => p.GetLeagueDataAsync(scenario.League.Id)).ReturnsAsync(scenario.League);
        leagueProcessor.Setup(p => p.GetUpcomingMatchesAsync(scenario.League.Id, scenario.League.CurrentSeason.Id))
            .ReturnsAsync(new LeagueUpcomingFixtures { scenario.Fixture });
        leagueProcessor.Setup(p => p.GetLeagueTableAsync(scenario.League.Id, scenario.League.CurrentSeason.Id))
            .ReturnsAsync(scenario.LeagueStanding);

        var matchRepository = new Mock<IExctimentMatchRepository>();
        matchRepository.Setup(r => r.GetActiveCompetitionsAsync())
            .ReturnsAsync(new List<Competition> { scenario.Competition });
        matchRepository.Setup(r => r.GetCompetitionActiveMatchesAsync(scenario.Competition.Id))
            .ReturnsAsync(new List<MatchEntity>());
        matchRepository.Setup(r => r.GetRivalryByTeamIdAsync(scenario.HomeTeam.Id, scenario.AwayTeam.Id))
            .ReturnsAsync(scenario.Rivalry);
        matchRepository.Setup(r => r.SaveTeamAsync(It.IsAny<Team>()))
            .ReturnsAsync((Team t) => t);

        MatchEntity? captured = null;
        matchRepository.Setup(r => r.SaveMatchAsync(It.IsAny<MatchEntity>()))
            .Callback<MatchEntity>(m => captured = m)
            .Returns(Task.CompletedTask);
        matchRepository.Setup(r => r.SaveHeadToHeadMatchesAsync(It.IsAny<List<HeadToHeadEntity>>()))
            .Returns(Task.CompletedTask);

        var processor = new ExcitementMatchProcessor(leagueProcessor.Object, matchRepository.Object, Mock.Of<ITelegramBot>());

        await processor.CalculateUpcomingMatchsExcitment();

        captured.Should().NotBeNull();
        captured!.ExcitmentScore.Should().BeApproximately(
            captured.CompetitionScore * LateCompetitionCoef +
            captured.FixtureScore * LateFixtureCoef +
            captured.FormScore * LateTeamFormCoef +
            captured.GoalsScore * LateTeamGoalsCoef +
            captured.CompetitionStandingScore * LateTableRankCoef +
            captured.HeadToHeadScore * LateHeadToHeadCoef +
            captured.TitleHolderScore * LateTitleHolderCoef +
            captured.RivalryScore * LateRivalryCoef,
            NumericTolerance);

        matchRepository.Verify(r => r.SaveMatchAsync(It.IsAny<MatchEntity>()), Times.Once);
    }
    private static Scenario CreateScenario()
    {
        var competition = new Competition
        {
            Id = 99,
            Name = "Test League",
            LeagueRanking = 0.8,
            IsActive = true,
            PrimaryColor = "#111111",
            BackgroundColor = "#222222",
            TitleHolderTeamId = 10
        };

        var league = new League
        {
            Id = competition.Id,
            Name = competition.Name,
            CurrentSeason = new LeagueSeason { Id = 2025, Name = "2024/25", Round = 19 },
            TitleHolder = new TeamTitleHolder { Id = competition.TitleHolderTeamId!.Value, Name = "Title Holders" },
            Ranking = competition.LeagueRanking
        };

        var homeFixtures = new TeamFixtureData
        {
            AmountOfGames = 5,
            Wins = 3,
            Draws = 2,
            GoalsFor = 12,
            GoalsAgainst = 5,
            FixtureResult = new List<MatchResultType>
            {
                MatchResultType.Win,
                MatchResultType.Win,
                MatchResultType.Win,
                MatchResultType.Draw,
                MatchResultType.Draw
            }
        };

        var awayFixtures = new TeamFixtureData
        {
            AmountOfGames = 5,
            Wins = 2,
            Draws = 3,
            GoalsFor = 8,
            GoalsAgainst = 6,
            FixtureResult = new List<MatchResultType>
            {
                MatchResultType.Win,
                MatchResultType.Draw,
                MatchResultType.Win,
                MatchResultType.Draw,
                MatchResultType.Draw
            }
        };

        var homeTeam = new TeamInfo
        {
            Id = 10,
            Name = "Home FC",
            LastFixtures = homeFixtures
        };

        var awayTeam = new TeamInfo
        {
            Id = 20,
            Name = "Away FC",
            LastFixtures = awayFixtures
        };

        var fixture = new UpcomingFixture
        {
            Id = 1234,
            MatchDate = DateTimeOffset.UtcNow.AddDays(1),
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HeadToHead = CreateHeadToHeadFixtures(homeTeam.Id, awayTeam.Id)
        };

        var leagueStanding = new LeagueStanding
        {
            LeagueId = league.Id,
            CurrentRound = 19,
            TotalRounds = 38,
            Standings = new List<Standing>
            {
                new() { Team = new TeamInfo { Id = homeTeam.Id }, Position = 1, Points = 25, Matches = 10 },
                new() { Team = new TeamInfo { Id = awayTeam.Id }, Position = 2, Points = 25, Matches = 10 }
            }
        };

        var rivalry = new Rivalry { RivarlyValue = 0.8 };

        return new Scenario(competition, league, homeTeam, awayTeam, fixture, leagueStanding, rivalry);
    }

    private static List<Fixture> CreateHeadToHeadFixtures(int homeTeamId, int awayTeamId)
    {
        return new List<Fixture>
        {
            new()
            {
                Id = 1,
                MatchDate = DateTimeOffset.UtcNow.AddDays(-30),
                HomeTeam = new TeamInfo { Id = homeTeamId },
                AwayTeam = new TeamInfo { Id = awayTeamId },
                HomeTeamScore = 2,
                AwayTeamScore = 1
            },
            new()
            {
                Id = 2,
                MatchDate = DateTimeOffset.UtcNow.AddDays(-20),
                HomeTeam = new TeamInfo { Id = homeTeamId },
                AwayTeam = new TeamInfo { Id = awayTeamId },
                HomeTeamScore = 1,
                AwayTeamScore = 1
            },
            new()
            {
                Id = 3,
                MatchDate = DateTimeOffset.UtcNow.AddDays(-15),
                HomeTeam = new TeamInfo { Id = awayTeamId },
                AwayTeam = new TeamInfo { Id = homeTeamId },
                HomeTeamScore = 3,
                AwayTeamScore = 3
            },
            new()
            {
                Id = 4,
                MatchDate = DateTimeOffset.UtcNow.AddDays(-10),
                HomeTeam = new TeamInfo { Id = awayTeamId },
                AwayTeam = new TeamInfo { Id = homeTeamId },
                HomeTeamScore = 0,
                AwayTeamScore = 0
            },
            new()
            {
                Id = 5,
                MatchDate = DateTimeOffset.UtcNow.AddDays(-5),
                HomeTeam = new TeamInfo { Id = awayTeamId },
                AwayTeam = new TeamInfo { Id = homeTeamId },
                HomeTeamScore = 0,
                AwayTeamScore = 2
            }
        };
    }

    private static double ComputeFixtureScoreBase(LeagueStanding leagueStanding)
    {
        if (leagueStanding.TotalRounds == 0)
        {
            return 0d;
        }

        return (double)leagueStanding.CurrentRound / leagueStanding.TotalRounds;
    }

    private static double ComputeFormScoreBase(TeamFixtureData homeFixtures, TeamFixtureData awayFixtures)
    {
        static double ComputeSingle(TeamFixtureData data) => ((data.Wins * 3d) + data.Draws) / 15d;

        return (ComputeSingle(homeFixtures) + ComputeSingle(awayFixtures)) / 2d;
    }

    private static double ComputeGoalsScoreBase(TeamFixtureData homeFixtures, TeamFixtureData awayFixtures)
    {
        static double ComputeSingle(TeamFixtureData data)
        {
            if (data.AmountOfGames == 0) return 0d;
            return (double)data.GoalsFor / data.AmountOfGames * 2.0;
        }

        return ComputeSingle(homeFixtures) + ComputeSingle(awayFixtures);
    }

    private static double ComputeTableScoreBase(TeamInfo homeTeam, TeamInfo awayTeam, LeagueStanding leagueStanding)
    {
        if (homeTeam == null || awayTeam == null || leagueStanding?.Standings == null || leagueStanding.Standings.Count == 0)
        {
            return 0d;
        }

        if (leagueStanding.CurrentRound <= 1)
        {
            return 0.5d;
        }

        var homeStanding = leagueStanding.Standings.FirstOrDefault(s => s.Team.Id == homeTeam.Id);
        var awayStanding = leagueStanding.Standings.FirstOrDefault(s => s.Team.Id == awayTeam.Id);

        if (homeStanding == null || awayStanding == null)
        {
            return 0d;
        }

        var positionDiff = Math.Abs(homeStanding.Position - awayStanding.Position) - 1d;
        var totalTeams = (double)leagueStanding.Standings.Count;

        var positionValue = 1d / (1d + (positionDiff / (totalTeams - 1d)));
        var averageTeamPosition = (homeStanding.Position + awayStanding.Position) / 2d;
        var topBottomMatchupValue = 1d - (averageTeamPosition - 1d) / (totalTeams - 1d);

        var pointDifference = Math.Abs(homeStanding.Points - awayStanding.Points);
        var maxPointDifference = (leagueStanding.TotalRounds - Math.Max(homeStanding.Matches, awayStanding.Matches)) * 3d;
        var pointImpactValue = 1d / (1d + (pointDifference / (maxPointDifference - 1d)));

        return positionValue * topBottomMatchupValue * pointImpactValue;
    }

    private static double ComputeHeadToHeadScoreBase(IEnumerable<Fixture> fixtures, int homeTeamId, int awayTeamId)
    {
        var fixtureList = fixtures?.ToList() ?? new List<Fixture>();
        if (fixtureList.Count == 0)
        {
            return 0.5d;
        }

        var homeWins = fixtureList.Count(c =>
            (c.HomeTeam.Id == homeTeamId && c.HomeTeamScore > c.AwayTeamScore) ||
            (c.AwayTeam.Id == homeTeamId && c.AwayTeamScore > c.HomeTeamScore));

        var awayWins = fixtureList.Count(c =>
            (c.HomeTeam.Id == awayTeamId && c.HomeTeamScore > c.AwayTeamScore) ||
            (c.AwayTeam.Id == awayTeamId && c.AwayTeamScore > c.HomeTeamScore));

        var drawGames = fixtureList.Count(c => c.HomeTeamScore == c.AwayTeamScore);

        var value = ((homeWins + awayWins) * 3d + drawGames) / 15d;
        return value > 1d ? 1d : value;
    }

    private static double ComputeTitleHolderScoreBase(TeamTitleHolder titleHolder, int homeTeamId, int awayTeamId)
    {
        if (titleHolder == null)
        {
            return 0d;
        }

        return titleHolder.Id == homeTeamId || titleHolder.Id == awayTeamId ? 1d : 0d;
    }

    private sealed record Scenario(
        Competition Competition,
        League League,
        TeamInfo HomeTeam,
        TeamInfo AwayTeam,
        UpcomingFixture Fixture,
        LeagueStanding LeagueStanding,
        Rivalry Rivalry);
}