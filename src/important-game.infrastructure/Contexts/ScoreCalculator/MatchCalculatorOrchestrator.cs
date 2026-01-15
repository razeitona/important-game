using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto;
using FuzzySharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    ISofaScoreIntegration sofaScoreIntegration,
    IExternalProvidersRepository externalProvidersRepository,
    IOptions<LiveScoreCalculatorOptions> liveScoreOptions,
    LiveScoreCalculator liveScoreCalculator,
    ILogger<MatchCalculatorOrchestrator> logger) : IMatchCalculatorOrchestrator
{
    private const int MaxConcurrentCalculations = 1;

    private readonly IMatchesRepository _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
    private readonly ICompetitionRepository _competitionRepository = competitionRepository ?? throw new ArgumentNullException(nameof(competitionRepository));
    private readonly IMatchCalculator _matchCalculator = matchCalculator ?? throw new ArgumentNullException(nameof(matchCalculator));
    private readonly ISofaScoreIntegration _sofaScoreIntegration = sofaScoreIntegration ?? throw new ArgumentNullException(nameof(sofaScoreIntegration));
    private readonly IExternalProvidersRepository _externalProvidersRepository = externalProvidersRepository ?? throw new ArgumentNullException(nameof(externalProvidersRepository));
    private readonly LiveScoreCalculatorOptions _liveScoreOptions = liveScoreOptions?.Value ?? throw new ArgumentNullException(nameof(liveScoreOptions));
    private readonly LiveScoreCalculator _liveScoreCalculator = liveScoreCalculator ?? throw new ArgumentNullException(nameof(liveScoreCalculator));
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

    public async Task CalculateLiveScoreAsync(CancellationToken cancellationToken = default)
    {
        if (!_liveScoreOptions.Enabled)
        {
            _logger.LogInformation("Live score calculator is disabled");
            return;
        }

        try
        {
            _logger.LogInformation("Starting live score calculation");

            // Step 1: Get internal live matches
            var internalLiveMatches = await _matchesRepository.GetLiveMatchesAsync();
            internalLiveMatches = internalLiveMatches.Where(c => c.StartTime.Date == DateTime.UtcNow.Date).ToList();

            if (internalLiveMatches == null || internalLiveMatches.Count == 0)
            {
                _logger.LogInformation("No live matches found");
                return;
            }

            _logger.LogInformation("Found {Count} live matches", internalLiveMatches.Count);

            // Step 2: Get SofaScore live matches
            SSLiveEventsResponse? sofascoreLiveMatches = null;
            try
            {
                sofascoreLiveMatches = await _sofaScoreIntegration.GetAllLiveMatchesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SofaScore live matches");
                return;
            }

            if (sofascoreLiveMatches?.Events == null || sofascoreLiveMatches.Events.Count == 0)
            {
                _logger.LogWarning("No live matches found on SofaScore");
                return;
            }

            _logger.LogInformation("Found {Count} live matches on SofaScore", sofascoreLiveMatches.Events.Count);

            // Step 3: Map IDs (fuzzy matching + persist)
            await MapLiveMatchIdsAsync(internalLiveMatches, sofascoreLiveMatches.Events, cancellationToken);

            // Step 4: Select priority matches
            var priorityMatches = SelectPriorityMatches(internalLiveMatches, _liveScoreOptions.MaxMatchesPerCycle);

            _logger.LogInformation("Processing {Count} priority matches", priorityMatches.Count);

            // Step 5: Update each priority match with live data (if enabled)
            if (_liveScoreOptions.UpdateStatistics)
            {
                var updatedCount = 0;
                foreach (var match in priorityMatches)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await UpdateMatchWithLiveDataAsync(match, cancellationToken);
                    updatedCount++;

                    // Add delay between statistics calls to avoid rate limiting
                    // Only delay if not the last match and delay is configured
                    if (updatedCount < priorityMatches.Count && _liveScoreOptions.DelayBetweenStatisticsCallsSeconds > 0)
                    {
                        _logger.LogDebug("Waiting {Seconds}s before next statistics call",
                            _liveScoreOptions.DelayBetweenStatisticsCallsSeconds);
                        await Task.Delay(TimeSpan.FromSeconds(_liveScoreOptions.DelayBetweenStatisticsCallsSeconds),
                            cancellationToken);
                    }
                }

                _logger.LogInformation("Live score calculation complete. Updated {Count} matches", updatedCount);
            }
            else
            {
                _logger.LogInformation("Statistics updates disabled. Only ID mapping performed.");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Live score calculation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during live score calculation");
        }
    }

    private async Task MapLiveMatchIdsAsync(
        List<LiveMatchDto> internalMatches,
        IReadOnlyList<SSEvent> sofascoreMatches,
        CancellationToken cancellationToken)
    {
        var mappedCount = 0;
        var unmappedCount = 0;

        // Load all existing mappings for current live matches once (performance optimization)
        var internalMatchIds = internalMatches.Select(m => m.MatchId).ToList();
        var existingMappings = new HashSet<string>();

        foreach (var matchId in internalMatchIds)
        {
            var mapping = await _externalProvidersRepository
                .GetExternalProviderMatchAsync(_liveScoreOptions.SofaScoreProviderId, matchId);
            if (mapping != null)
                existingMappings.Add(mapping.ExternalMatchId);
        }

        foreach (var ssMatch in sofascoreMatches)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Check if already mapped (using in-memory set - much faster)
                if (existingMappings.Contains(ssMatch.Id.ToString()))
                    continue; // Already mapped

                // Try fuzzy match
                var internalMatch = internalMatches.Where(c => c.StartTime.Date == DateTime.UtcNow.Date).FirstOrDefault(m =>
                    FuzzyMatchTeams(m, ssMatch) &&
                    MatchDates(m.StartTime, ssMatch.StartTimestamp));

                if (internalMatch != null)
                {
                    // Persist mapping
                    await _externalProvidersRepository.SaveExternalProviderMatchAsync(
                        new ExternalProviderMatchesEntity
                        {
                            ProviderId = _liveScoreOptions.SofaScoreProviderId,
                            InternalMatchId = internalMatch.MatchId,
                            ExternalMatchId = ssMatch.Id.ToString()
                        });

                    mappedCount++;
                    _logger.LogInformation(
                        "Mapped SofaScore event {SsId} to internal match {InternalId}: {Home} vs {Away}",
                        ssMatch.Id, internalMatch.MatchId, internalMatch.HomeTeamName, internalMatch.AwayTeamName);
                }
                else
                {
                    unmappedCount++;
                    _logger.LogDebug(
                        "Could not map SofaScore event {SsId}: {Home} vs {Away}",
                        ssMatch.Id, ssMatch.HomeTeam.Name, ssMatch.AwayTeam.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping SofaScore event {EventId}", ssMatch.Id);
            }
        }

        _logger.LogInformation(
            "ID mapping complete. Mapped: {Mapped}, Unmapped: {Unmapped}",
            mappedCount, unmappedCount);
    }

    private bool FuzzyMatchTeams(LiveMatchDto internalMatch, SSEvent sofascoreMatch)
    {
        var homeNormalized = NormalizeTeamName(internalMatch.HomeTeamShortName);
        var awayNormalized = NormalizeTeamName(internalMatch.AwayTeamShortName);
        var ssHomeNormalized = NormalizeTeamName(sofascoreMatch.HomeTeam.ShortName ?? sofascoreMatch.HomeTeam.Name);
        var ssAwayNormalized = NormalizeTeamName(sofascoreMatch.AwayTeam.ShortName ?? sofascoreMatch.AwayTeam.Name);

        var homeMatch = Fuzz.Ratio(homeNormalized, ssHomeNormalized) >= _liveScoreOptions.FuzzyMatchThreshold;
        var awayMatch = Fuzz.Ratio(awayNormalized, ssAwayNormalized) >= _liveScoreOptions.FuzzyMatchThreshold;

        return homeMatch && awayMatch;
    }

    private static string NormalizeTeamName(string teamName)
    {
        return teamName
            .ToLowerInvariant()
            .Replace("fc", "")
            .Replace("cf", "")
            .Replace("sc", "")
            .Replace(".", "")
            .Replace("-", " ")
            .Trim();
    }

    private bool MatchDates(DateTimeOffset internalDate, int sofascoreTimestamp)
    {
        var ssDate = DateTimeOffset.FromUnixTimeSeconds(sofascoreTimestamp);
        var difference = Math.Abs((internalDate - ssDate).TotalHours);
        return difference <= _liveScoreOptions.DateToleranceHours;
    }

    private static List<LiveMatchDto> SelectPriorityMatches(List<LiveMatchDto> matches, int maxCount)
    {
        return matches
            .OrderByDescending(m => m.CurrentExcitementScore)
            .ThenByDescending(m => m.StartTime)
            .ToList();
    }

    private async Task UpdateMatchWithLiveDataAsync(LiveMatchDto match, CancellationToken cancellationToken)
    {
        try
        {
            // Get SofaScore event ID mapping
            var mapping = await _externalProvidersRepository
                .GetExternalProviderMatchAsync(_liveScoreOptions.SofaScoreProviderId, match.MatchId);

            if (mapping == null)
            {
                _logger.LogDebug("No SofaScore mapping for match {MatchId}", match.MatchId);
                return;
            }

            // Get live event information (includes current score)
            var eventInfo = await _sofaScoreIntegration.GetEventInformationAsync(mapping.ExternalMatchId);

            if (eventInfo?.EventData == null)
            {
                _logger.LogDebug("No event data available for match {MatchId}", match.MatchId);
                return;
            }

            // Get live statistics from SofaScore
            var statistics = await _sofaScoreIntegration.GetEventStatisticsAsync(mapping.ExternalMatchId);

            if (statistics?.Statistics == null || statistics.Statistics.Count == 0)
            {
                _logger.LogDebug("No statistics available for match {MatchId}", match.MatchId);
                return;
            }

            // Calculate new LiveExcitementScore using the LiveScoreCalculator with full context
            // Uses pre-match ExcitementScore as baseline, or previous LiveExcitementScore if exists
            // Includes current score and league positions for contextual analysis
            var (newLiveScore, components) = _liveScoreCalculator.CalculateLiveScore(
                baselineScore: match.CurrentExcitementScore,
                currentLiveScore: match.CurrentLiveExcitementScore,
                eventData: eventInfo.EventData,
                statistics: statistics,
                homeTeamPosition: match.HomeTeamPosition,
                awayTeamPosition: match.AwayTeamPosition);

            // Update database with new score and all components
            await _matchesRepository.UpdateLiveExcitementScoreAsync(match.MatchId, newLiveScore, components);

            _logger.LogInformation(
                "Updated match {MatchId} ({Home} vs {Away}): Score={HomeGoals}-{AwayGoals}, LiveScore={LiveScore} " +
                "(ScoreLine={SL}, xG={XG}, Fouls={F}, Cards={C}, Possession={P}, BigChances={BC})",
                match.MatchId, match.HomeTeamName, match.AwayTeamName,
                eventInfo.EventData.HomeScore.Current, eventInfo.EventData.AwayScore.Current, newLiveScore,
                components.ScoreLineScore, components.XGoalsScore, components.TotalFoulsScore,
                components.TotalCardsScore, components.PossessionScore, components.BigChancesScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update live data for match {MatchId}", match.MatchId);
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
