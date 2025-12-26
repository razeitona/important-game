using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.Matches.Data.Entities
{
    /// <summary>
    /// Stores team statistics for the last 5 matches in a specific competition.
    /// Used for enriching match excitement calculations with recent form data.
    /// </summary>
    public class TeamSeasonStats
    {
        public int Id { get; set; }

        public int TeamId { get; set; }

        public int CompetitionId { get; set; }

        /// <summary>
        /// Goals scored in last 5 matches.
        /// </summary>
        public int GoalsFor5 { get; set; }

        /// <summary>
        /// Goals conceded in last 5 matches.
        /// </summary>
        public int GoalsAgainst5 { get; set; }

        /// <summary>
        /// Wins in last 5 matches.
        /// </summary>
        public int Wins5 { get; set; }

        /// <summary>
        /// Draws in last 5 matches.
        /// </summary>
        public int Draws5 { get; set; }

        /// <summary>
        /// Losses in last 5 matches.
        /// </summary>
        public int Losses5 { get; set; }

        /// <summary>
        /// When this statistics snapshot was last updated.
        /// Used to determine if data needs refresh from Gemini API.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Team? Team { get; set; }

        public virtual CompetitionEntity? Competition { get; set; }

        /// <summary>
        /// Average goals scored per match (last 5).
        /// </summary>
        public double AverageGoalsFor => Wins5 + Draws5 + Losses5 > 0 
            ? (double)GoalsFor5 / (Wins5 + Draws5 + Losses5) 
            : 0;

        /// <summary>
        /// Average goals conceded per match (last 5).
        /// </summary>
        public double AverageGoalsAgainst => Wins5 + Draws5 + Losses5 > 0 
            ? (double)GoalsAgainst5 / (Wins5 + Draws5 + Losses5) 
            : 0;

        /// <summary>
        /// Win percentage in last 5 matches.
        /// </summary>
        public double WinPercentage => Wins5 + Draws5 + Losses5 > 0 
            ? (double)Wins5 / (Wins5 + Draws5 + Losses5) * 100 
            : 0;
    }
}
