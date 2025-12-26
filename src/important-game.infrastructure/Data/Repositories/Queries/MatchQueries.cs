namespace important_game.infrastructure.Data.Repositories.Queries
{
    /// <summary>
    /// SQL queries for Match repository operations.
    /// </summary>
    internal static class MatchQueries
    {
        internal const string CheckMatchExists = "SELECT COUNT(*) FROM match WHERE id = @Id";

        internal const string UpdateMatch = @"
            UPDATE match 
            SET competition_id = @CompetitionId,
                match_date_utc = @MatchDateUTC,
                home_team_id = @HomeTeamId,
                away_team_id = @AwayTeamId,
                home_team_position = @HomeTeamPosition,
                away_team_position = @AwayTeamPosition,
                home_score = @HomeScore,
                away_score = @AwayScore,
                home_form = @HomeForm,
                away_form = @AwayForm,
                match_status = @MatchStatus,
                excitement_score = @ExcitmentScore,
                competition_score = @CompetitionScore,
                fixture_score = @FixtureScore,
                form_score = @FormScore,
                goals_score = @GoalsScore,
                competition_standing_score = @CompetitionStandingScore,
                head_to_head_score = @HeadToHeadScore,
                rivalry_score = @RivalryScore,
                title_holder_score = @TitleHolderScore,
                updated_date_utc = @UpdatedDateUTC,
                gemini_stats_updated_at = @GeminiStatsUpdatedAt
            WHERE id = @Id";

        internal const string InsertMatch = @"
            INSERT INTO match (
                id, competition_id, match_date_utc, home_team_id, away_team_id,
                home_team_position, away_team_position, home_score, away_score,
                home_form, away_form, match_status, excitement_score,
                competition_score, fixture_score, form_score, goals_score,
                competition_standing_score, head_to_head_score, rivalry_score,
                title_holder_score, updated_date_utc, gemini_stats_updated_at
            ) VALUES (
                @Id, @CompetitionId, @MatchDateUTC, @HomeTeamId, @AwayTeamId,
                @HomeTeamPosition, @AwayTeamPosition, @HomeScore, @AwayScore,
                @HomeForm, @AwayForm, @MatchStatus, @ExcitmentScore,
                @CompetitionScore, @FixtureScore, @FormScore, @GoalsScore,
                @CompetitionStandingScore, @HeadToHeadScore, @RivalryScore,
                @TitleHolderScore, @UpdatedDateUTC, @GeminiStatsUpdatedAt
            )";

        internal const string SelectMatchById = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.id = @Id";

        internal const string SelectUpcomingMatches = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.match_status != @FinishedStatus
            ORDER BY m.match_date_utc ASC";

        internal const string SelectMatchesFromCompetition = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.competition_id = @CompetitionId
            ORDER BY m.match_date_utc ASC";

        internal const string SelectCompetitionActiveMatches = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.competition_id = @CompetitionId AND m.match_status != @FinishedStatus
            ORDER BY m.match_date_utc ASC";

        internal const string SelectUpcomingMatchesFromCompetition = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.competition_id = @CompetitionId AND m.match_status = @UpcomingStatus
            ORDER BY m.match_date_utc ASC";

        internal const string SelectLiveMatchesFromCompetition = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.competition_id = @CompetitionId AND m.match_status = @LiveStatus
            ORDER BY m.match_date_utc ASC";

        internal const string SelectUnfinishedMatches = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.match_status != @FinishedStatus
            ORDER BY m.match_date_utc ASC";

        internal const string SelectFinishedMatchesFromCompetition = @"
            SELECT 
                m.id AS Id,
                m.competition_id AS CompetitionId,
                m.match_date_utc AS MatchDateUTC,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.home_team_position AS HomeTeamPosition,
                m.away_team_position AS AwayTeamPosition,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.home_form AS HomeForm,
                m.away_form AS AwayForm,
                m.match_status AS MatchStatus,
                m.excitement_score AS ExcitmentScore,
                m.competition_score AS CompetitionScore,
                m.fixture_score AS FixtureScore,
                m.form_score AS FormScore,
                m.goals_score AS GoalsScore,
                m.competition_standing_score AS CompetitionStandingScore,
                m.head_to_head_score AS HeadToHeadScore,
                m.rivalry_score AS RivalryScore,
                m.title_holder_score AS TitleHolderScore,
                m.updated_date_utc AS UpdatedDateUTC,
                m.gemini_stats_updated_at AS GeminiStatsUpdatedAt
            FROM match m
            WHERE m.competition_id = @CompetitionId AND m.match_status = @FinishedStatus
            ORDER BY m.match_date_utc DESC";
    }
}
