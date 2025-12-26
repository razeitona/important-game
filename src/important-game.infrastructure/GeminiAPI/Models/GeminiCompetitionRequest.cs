namespace important_game.infrastructure.GeminiAPI.Models
{
    /// <summary>
    /// Request structure for Gemini API data enrichment.
    /// Contains all necessary information to fetch calendar, standings, and team statistics.
    /// </summary>
    public class GeminiCompetitionRequest
    {
        /// <summary>
        /// Competition identifier.
        /// </summary>
        public int CompetitionId { get; set; }

        /// <summary>
        /// Competition name.
        /// </summary>
        public string CompetitionName { get; set; } = string.Empty;

        /// <summary>
        /// Whether to request calendar data.
        /// </summary>
        public bool RequestCalendar { get; set; }

        /// <summary>
        /// Reason why calendar is being requested.
        /// </summary>
        public string CalendarReason { get; set; } = string.Empty;

        /// <summary>
        /// Whether to request competition table/standings.
        /// </summary>
        public bool RequestTable { get; set; }

        /// <summary>
        /// Reason why table is being requested.
        /// </summary>
        public string TableReason { get; set; } = string.Empty;

        /// <summary>
        /// Teams for which to request last 5 matches statistics.
        /// </summary>
        public List<GeminiTeamRequest> TeamRequests { get; set; } = new();
    }

    /// <summary>
    /// Team-specific request for statistics in Gemini API.
    /// </summary>
    public class GeminiTeamRequest
    {
        /// <summary>
        /// Team identifier.
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// Team name.
        /// </summary>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// Number of recent matches to analyze.
        /// </summary>
        public int LastDays { get; set; } = 5;
    }
}
