using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.ScoreCalculator.Calculators;
using important_game.infrastructure.Contexts.ScoreCalculator.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.ScoreCalculator;

/// <summary>
/// Calculates excitement scores for unfinished matches based on database data.
/// Uses composition pattern with specialized calculators for different score components.
/// Processes matches in parallel with semaphore control to manage database load.
/// </summary>
[ExcludeFromCodeCoverage]
internal class ExcitmentMatchCalculator : IExcitmentMatchCalculator
{
    private const int MaxConcurrentCalculations = 3;

    private readonly IMatchesRepository _matchesRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ILogger<ExcitmentMatchCalculator> _logger;

    // Calculators (Composition Pattern - Single Responsibility)
    private readonly IFixtureValueCalculator _fixtureCalculator;
    private readonly ITeamFormCalculator _teamFormCalculator;
    private readonly ILeagueTableValueCalculator _leagueTableCalculator;
    private readonly IHeadToHeadCalculator _headToHeadCalculator;
    private readonly ITitleHolderCalculator _titleHolderCalculator;
    private readonly ILeagueLateStageDetector _lateStageDetector;

    public ExcitmentMatchCalculator(
        IMatchesRepository matchesRepository,
        ICompetitionRepository competitionRepository,
        IFixtureValueCalculator fixtureCalculator,
        ITeamFormCalculator teamFormCalculator,
        ILeagueTableValueCalculator leagueTableCalculator,
        IHeadToHeadCalculator headToHeadCalculator,
        ITitleHolderCalculator titleHolderCalculator,
        ILeagueLateStageDetector lateStageDetector,
        ILogger<ExcitmentMatchCalculator> logger)
    {
        _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
        _competitionRepository = competitionRepository ?? throw new ArgumentNullException(nameof(competitionRepository));
        _fixtureCalculator = fixtureCalculator ?? throw new ArgumentNullException(nameof(fixtureCalculator));
        _teamFormCalculator = teamFormCalculator ?? throw new ArgumentNullException(nameof(teamFormCalculator));
        _leagueTableCalculator = leagueTableCalculator ?? throw new ArgumentNullException(nameof(leagueTableCalculator));
        _headToHeadCalculator = headToHeadCalculator ?? throw new ArgumentNullException(nameof(headToHeadCalculator));
        _titleHolderCalculator = titleHolderCalculator ?? throw new ArgumentNullException(nameof(titleHolderCalculator));
        _lateStageDetector = lateStageDetector ?? throw new ArgumentNullException(nameof(lateStageDetector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CalculateExcitmentScoreAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting excitement score calculation for unfinished matches");

            var unfinishedMatches = await _matchesRepository.GetUnfinishedMatchesAsync();

            if (unfinishedMatches == null || unfinishedMatches.Count == 0)
            {
                _logger.LogInformation("No unfinished matches found for calculation");
                return;
            }

            _logger.LogInformation("Found {MatchCount} unfinished matches for calculation", unfinishedMatches.Count);

            using var semaphore = new SemaphoreSlim(MaxConcurrentCalculations, MaxConcurrentCalculations);
            List<Task> tasks = new List<Task>();
            foreach (var unfinishedMatch in unfinishedMatches)
            {
                if (DateTimeOffset.UtcNow < unfinishedMatch.UpdatedDateUTC.AddHours(2))
                    continue;
                
                var task = CalculateAndSaveMatchAsync(unfinishedMatch, semaphore, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Completed excitement score calculation for {MatchCount} matches", unfinishedMatches.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating excitement scores");
            throw;
        }
    }

    private async Task CalculateAndSaveMatchAsync(
        MatchesEntity match,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var competition = await _competitionRepository.GetCompetitionByIdAsync(match.CompetitionId!.Value);
            if (competition == null)
            {
                _logger.LogWarning("Could not find competition data for match {MatchId}", match.MatchId);
                return;
            }

            var season = match.SeasonId.HasValue ? await _competitionRepository.GetCompetitionSeasonByIdAsync(match.SeasonId.Value) : null;
            var competitionTable = await _competitionRepository.GetCompetitionTableAsync(match.CompetitionId.Value, match.SeasonId.Value);
            if (competitionTable == null)
            {
                _logger.LogWarning("Could not find competition table data for match {MatchId}", match.MatchId);
                return;
            }

            var rivarlyInformation = await _matchesRepository.GetRivalryAsync(match.HomeTeamId, match.AwayTeamId);
            var excitementScores = CalculateMatchScores(match, competition, season, competitionTable, rivarlyInformation);

            if (excitementScores == null)
                return;

            await _matchesRepository.UpdateMatchCalculatorAsync(excitementScores);

            _logger.LogDebug("Calculated excitement score {Score} for match {MatchId}", excitementScores.ExcitmentScore, match.MatchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating excitement score for match {MatchId}", match.MatchId);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private MatchCalcsDto? CalculateMatchScores(
        MatchesEntity match,
        CompetitionEntity competition,
        CompetitionSeasonsEntity? season,
        List<CompetitionTableEntity> competitionTable,
        RivalryEntity rivalryInformation)
    {

        var homeTeamTable = competitionTable.FirstOrDefault(c => c.TeamId == match.HomeTeamId);
        var awayTeamTable = competitionTable.FirstOrDefault(c => c.TeamId == match.AwayTeamId);

        if (homeTeamTable == null || awayTeamTable == null)
        {
            _logger.LogWarning("Could not find competition table data for match {MatchId}", match.MatchId);
            return default;
        }

        var matchResult = new MatchCalcsDto();
        matchResult.MatchId = match.MatchId;

        matchResult.CompetitionScore = competition.LeagueRanking;
        matchResult.FixtureScore = _fixtureCalculator.CalculateFixtureValue(match.Round, season.NumberOfRounds); // Standard season length
        matchResult.FormScore = (_teamFormCalculator.CalculateTeamFormScore(homeTeamTable.Wins, homeTeamTable.Draws, homeTeamTable.Matches) +
                           _teamFormCalculator.CalculateTeamFormScore(awayTeamTable.Wins, awayTeamTable.Draws, awayTeamTable.Matches)) / 2d;
        matchResult.GoalsScore = _teamFormCalculator.CalculateTeamGoalsScore(homeTeamTable.GoalsFor, homeTeamTable.Matches) +
                           _teamFormCalculator.CalculateTeamGoalsScore(awayTeamTable.GoalsFor, awayTeamTable.Matches);
        matchResult.CompetitionStandingScore = _leagueTableCalculator.CalculateLeagueTableValue(
            homeTeamTable.Position,
            awayTeamTable.Position,
            homeTeamTable.Points,
            awayTeamTable.Points,
            homeTeamTable.Matches,
            awayTeamTable.Matches,
            20, // Assume 20 teams
            38);
        matchResult.HeadToHeadScore = _headToHeadCalculator.CalculateHeadToHeadScore(0, 0, 0); // No h2h data in table
        matchResult.TitleHolderScore = _titleHolderCalculator.CalculateTitleHolderScore(match.HomeTeamId, match.AwayTeamId, season.TitleHolderId);
        matchResult.RivalryScore = rivalryInformation?.RivarlyValue ?? 0d;

        var isLateStage = _lateStageDetector.IsLateStage(homeTeamTable.Position, season.NumberOfRounds, competitionTable.Count);

        matchResult.ExcitmentScore =
            matchResult.CompetitionScore * CalculatorCoeficients.CompetitionCoef +
            matchResult.FixtureScore * CalculatorCoeficients.FixtureCoef +
            matchResult.FormScore * CalculatorCoeficients.TeamFormCoef +
            matchResult.GoalsScore * CalculatorCoeficients.TeamGoalsCoef +
            matchResult.CompetitionStandingScore * CalculatorCoeficients.TableRankCoef +
            matchResult.HeadToHeadScore * CalculatorCoeficients.HeadToHeadCoef +
            matchResult.TitleHolderScore * CalculatorCoeficients.TitleHolderCoef +
            matchResult.RivalryScore * CalculatorCoeficients.RivalryCoef;

        return matchResult;
    }
}
