namespace important_game.infrastructure.Contexts.ScoreCalculator
{
    /// <summary>
    /// Configuration options for the Live Score Calculator job.
    /// </summary>
    public class LiveScoreCalculatorOptions
    {
        /// <summary>
        /// Whether the live score calculator is enabled.
        /// Default: true
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Interval in minutes between live score calculations.
        /// Default: 10 minutes
        /// </summary>
        public int IntervalMinutes { get; set; } = 10;

        /// <summary>
        /// Maximum number of priority matches to update with detailed statistics per cycle.
        /// Default: 10 matches
        /// </summary>
        public int MaxMatchesPerCycle { get; set; } = 10;

        /// <summary>
        /// Fuzzy matching similarity threshold (0-100) for team name matching.
        /// Default: 85%
        /// </summary>
        public int FuzzyMatchThreshold { get; set; } = 80;

        /// <summary>
        /// Tolerance in hours for match date/time matching.
        /// Allows for timezone differences and delayed kick-offs.
        /// Default: 2 hours
        /// </summary>
        public int DateToleranceHours { get; set; } = 2;

        /// <summary>
        /// Provider ID for SofaScore in the ExternalProviders table.
        /// Default: 1
        /// </summary>
        public int SofaScoreProviderId { get; set; } = 2;

        /// <summary>
        /// Delay in seconds between individual match statistics API calls.
        /// Prevents rate limiting by spacing out requests.
        /// Default: 5 seconds (should be less than 30s since SofaScoreRateLimiter already enforces 30s)
        /// </summary>
        public int DelayBetweenStatisticsCallsSeconds { get; set; } = 5;

        /// <summary>
        /// Whether to update statistics for live matches or only do ID mapping.
        /// Set to false to minimize API calls if being rate limited.
        /// Default: true
        /// </summary>
        public bool UpdateStatistics { get; set; } = true;
    }
}
