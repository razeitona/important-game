namespace important_game.infrastructure.Contexts.Matches.Data.Entities
{
    /// <summary>
    /// Represents the individual components that make up the LiveExcitementScore.
    /// Each component is calculated from live match statistics and contributes to the overall live score.
    /// All scores are in 0-100 range before final composition.
    /// </summary>
    public class LiveScoreComponents
    {
        /// <summary>
        /// Score based on goal difference and match competitiveness.
        /// Higher for close, high-scoring games. Range: 0-30 points.
        /// </summary>
        public double ScoreLineScore { get; set; }

        /// <summary>
        /// Score based on Expected Goals (xG) difference between teams.
        /// Reflects quality of chances created. Range: 0-15 points.
        /// </summary>
        public double XGoalsScore { get; set; }

        /// <summary>
        /// Score based on total fouls committed by both teams.
        /// Reflects match intensity and physicality. Range: 0-10 points.
        /// </summary>
        public double TotalFoulsScore { get; set; }

        /// <summary>
        /// Score based on yellow and red cards shown.
        /// Reflects match intensity and drama. Range: 0-15 points.
        /// </summary>
        public double TotalCardsScore { get; set; }

        /// <summary>
        /// Score based on ball possession balance between teams.
        /// Higher for balanced possession (competitive match). Range: 0-15 points.
        /// </summary>
        public double PossessionScore { get; set; }

        /// <summary>
        /// Score based on big chances created by both teams.
        /// Reflects attacking quality and danger. Range: 0-15 points.
        /// </summary>
        public double BigChancesScore { get; set; }

        /// <summary>
        /// Calculates the total live bonus from all components.
        /// Total range: 0-100 points.
        /// </summary>
        public double TotalLiveBonus { get; set; }
    }
}
