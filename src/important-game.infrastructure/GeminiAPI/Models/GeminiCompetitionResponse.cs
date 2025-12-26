namespace important_game.infrastructure.GeminiAPI.Models
{
    /// <summary>
    /// Response structure from Gemini API containing enriched competition data.
    /// </summary>
    public class GeminiCompetitionResponse
    {
        /// <summary>
        /// Competition identifier.
        /// </summary>
        public int CompetitionId { get; set; }

        /// <summary>
        /// Calendar data (upcoming fixtures).
        /// </summary>
        public GeminiCalendar? Calendar { get; set; }

        /// <summary>
        /// Competition standings/table.
        /// </summary>
        public GeminiStandings? Standings { get; set; }

        /// <summary>
        /// Team statistics collection.
        /// </summary>
        public List<GeminiTeamStatistics> TeamStatistics { get; set; } = new();

        /// <summary>
        /// Timestamp when response was generated.
        /// </summary>
        public DateTime ResponseGeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Calendar data from Gemini API.
    /// </summary>
    public class GeminiCalendar
    {
        /// <summary>
        /// List of upcoming fixtures.
        /// </summary>
        public List<GeminiFixture> Fixtures { get; set; } = new();
    }

    /// <summary>
    /// Individual fixture from calendar.
    /// </summary>
    public class GeminiFixture
    {
        public int Id { get; set; }
        public int HomeTeamId { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public int AwayTeamId { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
        public DateTime MatchDateUTC { get; set; }
    }

    /// <summary>
    /// Competition standings from Gemini API.
    /// </summary>
    public class GeminiStandings
    {
        /// <summary>
        /// List of team standings ordered by position.
        /// </summary>
        public List<GeminiTeamStanding> Teams { get; set; } = new();
    }

    /// <summary>
    /// Individual team standing row.
    /// </summary>
    public class GeminiTeamStanding
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Points { get; set; }
        public int Matches { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
    }

    /// <summary>
    /// Team statistics for the last 5 matches.
    /// </summary>
    public class GeminiTeamStatistics
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int GoalsFor5 { get; set; }
        public int GoalsAgainst5 { get; set; }
        public int Wins5 { get; set; }
        public int Draws5 { get; set; }
        public int Losses5 { get; set; }
    }
}
