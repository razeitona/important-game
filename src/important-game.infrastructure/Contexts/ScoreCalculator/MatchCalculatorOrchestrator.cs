using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.ScoreCalculator;

/// <summary>
/// Calculates excitement scores for unfinished matches based on database data.
/// Uses composition pattern with specialized calculators for different score components.
/// Processes matches in parallel with semaphore control to manage database load.
/// </summary>
[ExcludeFromCodeCoverage]
internal class MatchCalculatorOrchestrator(
    IMatchesRepository matchesRepository,
    ICompetitionRepository competitionRepository,
    IMatchCalculator matchCalculator,
    ILogger<MatchCalculatorOrchestrator> logger) : IMatchCalculatorOrchestrator
{
    private const int MaxConcurrentCalculations = 1;

    private readonly IMatchesRepository _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
    private readonly ICompetitionRepository _competitionRepository = competitionRepository ?? throw new ArgumentNullException(nameof(competitionRepository));
    private readonly IMatchCalculator _matchCalculator = matchCalculator ?? throw new ArgumentNullException(nameof(matchCalculator));
    private readonly ILogger<MatchCalculatorOrchestrator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly ConcurrentDictionary<int, List<CompetitionTableEntity>> _competitionTableCache = new();
    private readonly ConcurrentDictionary<string, RivalryEntity> _rivalryCache = new();

    public async Task CalculateExcitmentScoreAsync(bool skipDateCondition = false, CancellationToken cancellationToken = default)
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
            List<Task> tasks = [];
            foreach (var unfinishedMatch in unfinishedMatches)
            {
                if (!skipDateCondition && DateTimeOffset.UtcNow < unfinishedMatch.UpdatedDateUTC.AddHours(2))
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

    private async Task CalculateAndSaveMatchAsync(UnfinishedMatchDto match,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var competitionTable = await TryGetCompetitionTableAsync(match.CompetitionId, match.SeasonId);
            if (competitionTable == null)
            {
                _logger.LogWarning("Could not find competition table data for match {MatchId}", match.MatchId);
                return;
            }

            var rivarlyInformation = await TryGetTeamRivalryAsync(match.HomeTeamId, match.AwayTeamId);
            var headToHeadMatches = await _matchesRepository.GetHeadToHeadMatchesAsync(match.HomeTeamId, match.AwayTeamId);
            var homeLastMatches = await _matchesRepository.GetRecentMatchesForTeamAsync(match.HomeTeamId, 5);
            var awayLastMatches = await _matchesRepository.GetRecentMatchesForTeamAsync(match.AwayTeamId, 5);
            var excitementScores = _matchCalculator.CalculateMatchScore(match, competitionTable, rivarlyInformation, headToHeadMatches, homeLastMatches, awayLastMatches);

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

    private async Task<RivalryEntity?> TryGetTeamRivalryAsync(int teamOneId, int teamTwoId)
    {
        if (_rivalryCache.TryGetValue($"{teamOneId}_{teamTwoId}", out var cachedRivalry))
            return cachedRivalry;
        if (_rivalryCache.TryGetValue($"{teamOneId}_{teamOneId}", out cachedRivalry))
            return cachedRivalry;

        var rivalryInformation = await _matchesRepository.GetRivalryAsync(teamOneId, teamTwoId);
        if (rivalryInformation == null)
            return default;

        _rivalryCache.TryAdd($"{teamOneId}_{teamTwoId}", rivalryInformation);
        _rivalryCache.TryAdd($"{teamTwoId}_{teamOneId}", rivalryInformation);
        return rivalryInformation;
    }

    private async Task<List<CompetitionTableEntity>> TryGetCompetitionTableAsync(int competitionId, int seasonId)
    {
        if (_competitionTableCache.TryGetValue(competitionId, out var cachedTable))
            return cachedTable;

        var competitionTable = await _competitionRepository.GetCompetitionTableAsync(competitionId, seasonId);
        _competitionTableCache.TryAdd(competitionId, competitionTable);
        return competitionTable;
    }
}
