namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for HeadToHead repository operations.
    /// </summary>
    internal static class HeadToHeadQueries
    {
        internal const string DeleteHeadToHeadByMatchId = "DELETE FROM headtohead WHERE match_id = @MatchId";

        internal const string InsertHeadToHead = @"
            INSERT INTO headtohead (
                match_id, home_team_id, away_team_id, match_date_utc, 
                home_team_score, away_team_score
            ) VALUES (
                @MatchId, @HomeTeamId, @AwayTeamId, @MatchDateUTC,
                @HomeTeamScore, @AwayTeamScore
            )";
    }
}
