using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;

namespace important_game.infrastructure.Contexts.Competitions.Data.Entities
{
    /// <summary>
    /// Represents a snapshot of a team's position in a competition's standing table.
    /// Stores historical classification data for analytics and comparison.
    /// </summary>
    public class CompetitionTable
    {
        public int Id { get; set; }

        public int CompetitionId { get; set; }

        public int TeamId { get; set; }

        public int Position { get; set; }

        public int Points { get; set; }

        public int Matches { get; set; }

        public int Wins { get; set; }

        public int Draws { get; set; }

        public int Losses { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Competition? Competition { get; set; }

        public virtual Team? Team { get; set; }

        /// <summary>
        /// Goal difference (GoalsFor - GoalsAgainst).
        /// </summary>
        public int GoalDifference => GoalsFor - GoalsAgainst;
    }
}
