using Microsoft.Extensions.Logging;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto;
using FuzzySharp;

namespace important_game.infrastructure.Contexts.Matches.Services
{
    /// <summary>
    /// Service for updating excitement scores of live matches using SofaScore data.
    /// Demonstrates the optimized integration workflow.
    /// </summary>
    public class LiveMatchExcitementService
    {
        private readonly ISofaScoreIntegration _sofaScoreIntegration;
        private readonly IMatchesRepository _matchesRepository;
        private readonly IExternalProvidersRepository _externalProvidersRepository;
        private readonly ILogger<LiveMatchExcitementService> _logger;

        private const int SOFASCORE_PROVIDER_ID = 1; // Adjust based on your database

        public LiveMatchExcitementService(
            ISofaScoreIntegration sofaScoreIntegration,
            IMatchesRepository matchesRepository,
            IExternalProvidersRepository externalProvidersRepository,
            ILogger<LiveMatchExcitementService> logger)
        {
            _sofaScoreIntegration = sofaScoreIntegration ?? throw new ArgumentNullException(nameof(sofaScoreIntegration));
            _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
            _externalProvidersRepository = externalProvidersRepository ?? throw new ArgumentNullException(nameof(externalProvidersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Main workflow: Updates excitement scores for currently live matches.
        /// This is what you would call from a background job every 10-30 minutes.
        /// </summary>
        public async Task UpdateLiveMatchExcitementScoresAsync()
        {
            _logger.LogInformation("Starting live match excitement score update");

            try
            {
                // Step 1: Get ALL live matches from SofaScore (1 request)
                var sofaScoreLiveMatches = await _sofaScoreIntegration.GetAllLiveMatchesAsync();

                if (sofaScoreLiveMatches?.Events == null || !sofaScoreLiveMatches.Events.Any())
                {
                    _logger.LogInformation("No live matches found on SofaScore");
                    return;
                }

                _logger.LogInformation("Found {Count} live matches on SofaScore", sofaScoreLiveMatches.Events.Count);

                // Step 2: Map SofaScore event IDs to internal match IDs
                await MapSofaScoreIdsToInternalMatchesAsync(sofaScoreLiveMatches.Events);

                // Step 3: Get mapped matches with priority
                var priorityMatches = await GetPriorityLiveMatchesAsync(topN: 10);

                if (!priorityMatches.Any())
                {
                    _logger.LogInformation("No internal matches mapped to live SofaScore events");
                    return;
                }

                _logger.LogInformation("Updating {Count} priority live matches", priorityMatches.Count);

                // Step 4: Update statistics for priority matches
                foreach (var match in priorityMatches)
                {
                    await UpdateMatchExcitementWithLiveDataAsync(match);
                }

                _logger.LogInformation("Live match excitement score update completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update live match excitement scores");
                throw;
            }
        }

        /// <summary>
        /// Maps SofaScore event IDs to internal match IDs using fuzzy matching.
        /// Only maps if not already mapped.
        /// </summary>
        private async Task MapSofaScoreIdsToInternalMatchesAsync(IReadOnlyList<SSEvent> sofascoreMatches)
        {
            // Get our internal live matches (you'll need to implement this query)
            var internalLiveMatches = await GetInternalLiveMatchesAsync();

            var mappedCount = 0;
            var unmappedCount = 0;

            foreach (var ssMatch in sofascoreMatches)
            {
                // Check if already mapped
                var existingMapping = await _externalProvidersRepository
                    .GetExternalProviderMatchesByProviderAsync(SOFASCORE_PROVIDER_ID);

                if (existingMapping.Any(m => m.ExternalMatchId == ssMatch.Id.ToString()))
                {
                    continue; // Already mapped
                }

                // Try to find matching internal match
                var internalMatch = internalLiveMatches.FirstOrDefault(m =>
                    IsTeamMatch(m.HomeTeam, ssMatch.HomeTeam.Name) &&
                    IsTeamMatch(m.AwayTeam, ssMatch.AwayTeam.Name) &&
                    IsDateMatch(m.StartTime, ssMatch.StartTimestamp));

                if (internalMatch != null)
                {
                    // Save mapping using the correct method name
                    await _externalProvidersRepository.SaveExternalProviderMatchAsync(
                        new ExternalProviderMatchesEntity
                        {
                            ProviderId = SOFASCORE_PROVIDER_ID,
                            InternalMatchId = internalMatch.Id,
                            ExternalMatchId = ssMatch.Id.ToString()
                        });

                    mappedCount++;
                    _logger.LogInformation(
                        "Mapped SofaScore event {SsId} to internal match {InternalId}: {Home} vs {Away}",
                        ssMatch.Id, internalMatch.Id, internalMatch.HomeTeam, internalMatch.AwayTeam);
                }
                else
                {
                    unmappedCount++;
                    _logger.LogWarning(
                        "Could not map SofaScore event {SsId}: {Home} vs {Away}",
                        ssMatch.Id, ssMatch.HomeTeam.Name, ssMatch.AwayTeam.Name);
                }
            }

            _logger.LogInformation(
                "ID Mapping complete. Mapped: {Mapped}, Unmapped: {Unmapped}",
                mappedCount, unmappedCount);
        }

        /// <summary>
        /// Updates a single match's excitement score with live SofaScore data.
        /// </summary>
        private async Task UpdateMatchExcitementWithLiveDataAsync(LiveMatchWithMapping match)
        {
            try
            {
                // Get live statistics from SofaScore
                var statistics = await _sofaScoreIntegration.GetEventStatisticsAsync(match.SofaScoreEventId);

                if (statistics?.Statistics == null)
                {
                    _logger.LogWarning("No statistics available for match {MatchId}", match.InternalMatchId);
                    return;
                }

                // Calculate live excitement bonus
                var liveBonus = CalculateLiveExcitementBonus(statistics);

                // Update match excitement score (you'll need to implement this)
                // await _matchCalculator.UpdateLiveExcitementAsync(match.InternalMatchId, liveBonus);

                _logger.LogInformation(
                    "Updated match {MatchId} ({Home} vs {Away}) with live bonus: +{Bonus}",
                    match.InternalMatchId, match.HomeTeam, match.AwayTeam, liveBonus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update excitement for match {MatchId}",
                    match.InternalMatchId);
            }
        }

        /// <summary>
        /// Calculates excitement bonus based on live match statistics.
        /// </summary>
        private int CalculateLiveExcitementBonus(SSEventStatistics statistics)
        {
            int bonus = 0;

            // Extract statistics from all periods
            foreach (var period in statistics.Statistics)
            {
                foreach (var group in period.Groups)
                {
                    foreach (var stat in group.StatisticsItems)
                    {
                        // Goals (already in base score, but worth emphasizing)
                        if (stat.Name == "Goals")
                        {
                            var totalGoals = (int)(stat.HomeValue + stat.AwayValue);
                            bonus += totalGoals * 15; // +15 per goal
                        }

                        // Yellow cards (indicate intensity)
                        if (stat.Name == "Yellow cards")
                        {
                            var totalYellowCards = (int)(stat.HomeValue + stat.AwayValue);
                            bonus += totalYellowCards * 5; // +5 per yellow card
                        }

                        // Red cards (major events)
                        if (stat.Name == "Red cards")
                        {
                            var totalRedCards = (int)(stat.HomeValue + stat.AwayValue);
                            bonus += totalRedCards * 20; // +20 per red card
                        }

                        // Total shots (attacking game)
                        if (stat.Name == "Total shots")
                        {
                            var totalShots = (int)(stat.HomeValue + stat.AwayValue);
                            if (totalShots > 20) bonus += 10;
                            if (totalShots > 30) bonus += 10; // Very attacking
                        }

                        // Ball possession (balanced = exciting)
                        if (stat.Name == "Ball possession")
                        {
                            var possessionDiff = Math.Abs(stat.HomeValue - stat.AwayValue);
                            if (possessionDiff < 10) bonus += 8; // Very balanced
                            if (possessionDiff < 5) bonus += 12; // Extremely balanced
                        }

                        // Dangerous attacks
                        if (stat.Name == "Dangerous attacks")
                        {
                            var totalDangerousAttacks = (int)(stat.HomeValue + stat.AwayValue);
                            if (totalDangerousAttacks > 50) bonus += 10;
                        }

                        // Shots on target
                        if (stat.Name == "Shots on target")
                        {
                            var totalShotsOnTarget = (int)(stat.HomeValue + stat.AwayValue);
                            if (totalShotsOnTarget > 10) bonus += 8;
                        }
                    }
                }
            }

            return bonus;
        }

        /// <summary>
        /// Fuzzy matching for team names.
        /// Uses FuzzySharp library (already in dependencies).
        /// </summary>
        private bool IsTeamMatch(string internalTeamName, string sofascoreTeamName)
        {
            // Normalize names
            var normalized1 = NormalizeTeamName(internalTeamName);
            var normalized2 = NormalizeTeamName(sofascoreTeamName);

            // Exact match
            if (normalized1.Equals(normalized2, StringComparison.OrdinalIgnoreCase))
                return true;

            // Fuzzy match using Levenshtein distance
            var similarity = Fuzz.Ratio(normalized1, normalized2);
            return similarity >= 85; // 85% similarity threshold
        }

        /// <summary>
        /// Normalizes team name for matching.
        /// </summary>
        private string NormalizeTeamName(string teamName)
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

        /// <summary>
        /// Checks if match dates are close enough (within 2 hours tolerance).
        /// </summary>
        private bool IsDateMatch(DateTime internalDate, int sofascoreTimestamp)
        {
            var ssDate = DateTimeOffset.FromUnixTimeSeconds(sofascoreTimestamp).UtcDateTime;
            var difference = Math.Abs((internalDate - ssDate).TotalHours);
            return difference <= 2; // Allow 2 hours difference for timezone issues
        }

        /// <summary>
        /// Gets priority live matches that are mapped to SofaScore.
        /// Returns top N matches ordered by excitement score.
        /// </summary>
        private async Task<List<LiveMatchWithMapping>> GetPriorityLiveMatchesAsync(int topN)
        {
            // TODO: Implement this based on your repository
            // This is a placeholder - you'll need to join your matches table
            // with the ExternalProviderMatches table and filter by live status

            var result = new List<LiveMatchWithMapping>();

            // Example query logic:
            // 1. Get all mappings for SofaScore
            var mappings = await _externalProvidersRepository
                .GetExternalProviderMatchesByProviderAsync(SOFASCORE_PROVIDER_ID);

            // 2. Get corresponding internal matches that are live
            // 3. Order by excitement score
            // 4. Take top N
            // 5. Return with mapping info

            return result;
        }

        /// <summary>
        /// Gets internal matches that are currently live.
        /// </summary>
        private async Task<List<InternalLiveMatch>> GetInternalLiveMatchesAsync()
        {
            // TODO: Implement this based on your repository
            // Should return matches with status = "LIVE" or similar

            var result = new List<InternalLiveMatch>();
            return result;
        }

        // Helper classes
        private class LiveMatchWithMapping
        {
            public int InternalMatchId { get; set; }
            public string SofaScoreEventId { get; set; } = string.Empty;
            public string HomeTeam { get; set; } = string.Empty;
            public string AwayTeam { get; set; } = string.Empty;
            public int BaseExcitementScore { get; set; }
        }

        private class InternalLiveMatch
        {
            public int Id { get; set; }
            public string HomeTeam { get; set; } = string.Empty;
            public string AwayTeam { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
        }
    }
}
