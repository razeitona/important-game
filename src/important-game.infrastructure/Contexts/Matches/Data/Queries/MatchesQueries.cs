namespace important_game.infrastructure.Contexts.Matches.Data.Queries;
internal static class MatchesQueries
{
    internal const string CheckMatchExists = @"
        SELECT 
            COUNT(*) 
        FROM 
            Matches
        WHERE 
            HomeTeamId = @HomeTeamId
            AND AwayTeamId = @AwayTeamId
            AND MatchDateUTC = @MatchDateUTC;";

    internal const string InsertFinishedMatch = @"
        INSERT INTO Matches (
            CompetitionId, SeasonId, Round, MatchDateUTC, HomeTeamId, AwayTeamId, 
            HomeScore, AwayScore, IsFinished, UpdatedDateUTC
        ) VALUES (
            @CompetitionId, @SeasonId, @Round, @MatchDateUTC, @HomeTeamId, @AwayTeamId, 
            @HomeScore, @AwayScore, @IsFinished, @UpdatedDateUTC
        );
        SELECT last_insert_rowid();";

    internal const string UpdateFinishedMatch = @"
            UPDATE Matches 
            SET CompetitionId = @CompetitionId,
                SeasonId = @SeasonId,
                MatchDateUTC = @MatchDateUTC,
                HomeTeamId = @HomeTeamId,
                AwayTeamId = @AwayTeamId,
                HomeScore = @HomeScore,
                AwayScore = @AwayScore,
                IsFinished = @IsFinished,
                UpdatedDateUTC = @UpdatedDateUTC
            WHERE MatchId = @MatchId";

    internal const string UpdateMatchCalculators = @"
            UPDATE Matches 
            SET 
                HomeForm = @HomeForm,
                AwayForm = @AwayForm,
                ExcitmentScore = @ExcitmentScore,
                CompetitionScore = @CompetitionScore,
                FixtureScore = @FixtureScore,
                FormScore = @FormScore,
                GoalsScore = @GoalsScore,
                CompetitionStandingScore = @CompetitionStandingScore,
                HeadToHeadScore = @HeadToHeadScore,
                RivalryScore = @RivalryScore,
                TitleHolderScore = @TitleHolderScore,
                UpdatedDateUTC = @UpdatedDateUTC
            WHERE MatchId = @MatchId";

    internal const string UpsertFinishedMatches = @"
        INSERT INTO Matches (
            CompetitionId, SeasonId, MatchDateUTC, HomeTeamId, AwayTeamId, 
            HomeScore, AwayScore, IsFinished, UpdatedDateUTC
        ) VALUES (
            @CompetitionId, @SeasonId, @MatchDateUTC, @HomeTeamId, @AwayTeamId, 
            @HomeScore, @AwayScore, @IsFinished, @UpdatedDateUTC
        )
        ON CONFLICT(CompetitionId, SeasonId, MatchDateUTC, HomeTeamId, AwayTeamId) DO UPDATE SET
            HomeScore = excluded.HomeScore,
            AwayScore = excluded.AwayScore,
            IsFinished = excluded.IsFinished,
            UpdatedDateUTC = excluded.UpdatedDateUTC;";

    internal const string SelectUnfinishedMatches = @"
        SELECT *
        FROM Matches
        WHERE IsFinished = 0
        ORDER BY MatchDateUTC ASC";

    internal const string SelectAllUpcomingMatches = @"
        SELECT 
            m.MatchId,
            c.CompetitionId,
            c.Name as 'CompetitionName',
            c.PrimaryColor as 'CompetitionPrimaryColor',
            c.BackgroundColor as 'CompetitionBackgroundColor',
            m.MatchDateUTC,
            ht.Id as 'HomeTeamId',
            ht.Name as 'HomeTeamName',
            at.Name as 'AwayTeamName',
            at.Id as 'AwayTeamId',
            m.ExcitmentScore
        FROM Matches m
        INNER JOIN Teams ht
            ON m.HomeTeamId = ht.Id
        INNER JOIN Teams at
            ON m.AwayTeamId = at.Id
        INNER JOIN COmpetitions c
	        ON c.CompetitionId = m.CompetitionId
        WHERE m.IsFinished = 0
        ORDER BY m.MatchDateUTC ASC";
}
