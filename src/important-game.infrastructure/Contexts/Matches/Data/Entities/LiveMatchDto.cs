using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Matches.Data.Entities
{
    /// <summary>
    /// DTO representing a currently live football match.
    /// Used for live excitement score calculations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LiveMatchDto
    {
        public int MatchId { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public required string HomeTeamName { get; set; }
        public required string HomeTeamShortName { get; set; }
        public required string AwayTeamName { get; set; }
        public required string AwayTeamShortName { get; set; }
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Pre-match excitement score (0-100).
        /// Used as baseline for LiveExcitementScore calculation.
        /// </summary>
        public double CurrentExcitementScore { get; set; }

        /// <summary>
        /// Current live excitement score (0-100).
        /// Null if match hasn't been processed yet (will default to CurrentExcitementScore).
        /// </summary>
        public double? CurrentLiveExcitementScore { get; set; }

        public int CompetitionId { get; set; }
        public required string CompetitionName { get; set; }

        /// <summary>
        /// Home team position in the league table.
        /// 999 if no standings data available.
        /// </summary>
        public int HomeTeamPosition { get; set; }

        /// <summary>
        /// Away team position in the league table.
        /// 999 if no standings data available.
        /// </summary>
        public int AwayTeamPosition { get; set; }
    }
}
