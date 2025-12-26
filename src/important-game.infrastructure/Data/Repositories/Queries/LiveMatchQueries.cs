namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for LiveMatch repository operations.
    /// </summary>
    internal static class LiveMatchQueries
    {
        internal const string CheckLiveMatchExists = "SELECT COUNT(*) FROM live_match WHERE id = @Id";

        internal const string UpdateLiveMatch = @"
            UPDATE live_match 
            SET excitement_score = @ExcitmentScore,
                score_line_score = @ScoreLineScore,
                shot_target_score = @ShotTargetScore,
                x_goals_score = @XGoalsScore,
                total_fouls_score = @TotalFoulsScore,
                total_cards_score = @TotalCardsScore,
                possession_score = @PossesionScore,
                big_chances_score = @BigChancesScore
            WHERE id = @Id";

        internal const string InsertLiveMatch = @"
            INSERT INTO live_match (
                match_id, registered_date, home_score, away_score, minutes,
                excitement_score, score_line_score, shot_target_score, x_goals_score,
                total_fouls_score, total_cards_score, possession_score, big_chances_score
            ) VALUES (
                @MatchId, @RegisteredDate, @HomeScore, @AwayScore, @Minutes,
                @ExcitmentScore, @ScoreLineScore, @ShotTargetScore, @XGoalsScore,
                @TotalFoulsScore, @TotalCardsScore, @PossesionScore, @BigChancesScore
            )";

        internal const string SelectLiveMatchById = @"
            SELECT 
                id AS Id,
                match_id AS MatchId,
                registered_date AS RegisteredDate,
                home_score AS HomeScore,
                away_score AS AwayScore,
                minutes AS Minutes,
                excitement_score AS ExcitmentScore,
                score_line_score AS ScoreLineScore,
                shot_target_score AS ShotTargetScore,
                x_goals_score AS XGoalsScore,
                total_fouls_score AS TotalFoulsScore,
                total_cards_score AS TotalCardsScore,
                possession_score AS PossesionScore,
                big_chances_score AS BigChancesScore
            FROM live_match 
            WHERE id = @Id";
    }
}
