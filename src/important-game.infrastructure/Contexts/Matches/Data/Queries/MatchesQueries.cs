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
        SELECT 
	        m.MatchId,
	        m.CompetitionId,
	        m.SeasonId,
            cs.TitleHolderId,
	        c.LeagueRanking,
	        m.MatchDateUTC,
	        m.Round,
	        cs.NumberOfRounds,
	        m.HomeTeamId,
	        m.AwayTeamId,
	        m.UpdatedDateUTC
        FROM Matches m
        INNER JOIN Competitions c
	        ON c.CompetitionId = m.CompetitionId
        INNER JOIN CompetitionSeasons cs
	        ON cs.CompetitionId = c.CompetitionId AND cs.SeasonId = m.SeasonId
        WHERE m.IsFinished = 0";

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

    internal const string SelectMatchById = @"
       SELECT 
           m.MatchId,
           m.CompetitionId,
           m.SeasonId,
           c.Name as 'CompetitionName',
           c.PrimaryColor as 'CompetitionPrimaryColor',
           c.BackgroundColor as 'CompetitionBackgroundColor',
           m.MatchDateUTC,
           ht.Id as 'HomeTeamId',
           ht.Name as 'HomeTeamName',
           hct.Position as 'HomeTeamTablePosition',
	       m.HomeForm as 'HomeTeamForm',
           at.Id as 'AwayTeamId',
	       at.Name as 'AwayTeamName',
           act.Position as 'AwayTeamTablePosition',
	       m.AwayForm as 'AwayTeamForm',
           m.ExcitmentScore,
	       m.CompetitionScore,
	       m.CompetitionStandingScore,
	       m.FixtureScore,
	       m.FormScore,
	       m.GoalsScore,
	       m.HeadToHeadScore,
	       m.RivalryScore,
	       m.TitleHolderScore
       FROM Matches m
       INNER JOIN Teams ht
           ON m.HomeTeamId = ht.Id
       INNER JOIN Teams at
           ON m.AwayTeamId = at.Id
       INNER JOIN Competitions c
           ON c.CompetitionId = m.CompetitionId
       LEFT JOIN CompetitionTable hct
           ON hct.CompetitionId = c.CompetitionId
	       AND hct.TeamId = ht.Id
       LEFT JOIN CompetitionTable act
           ON act.CompetitionId = c.CompetitionId
	       AND act.TeamId = at.Id
        WHERE m.MatchId = @MatchId";

    internal const string SelectHeadToHeadMatches = @"
        SELECT 
            m.MatchDateUTC,
            m.HomeTeamId,
            ht.Name as 'HomeTeamName',
            m.HomeScore as 'HomeTeamScore',
            m.AwayTeamId,
            at.Name as 'AwayTeamName',
            m.AwayScore as 'AwayTeamScore'
        FROM Matches m
        INNER JOIN Teams ht
            ON m.HomeTeamId = ht.Id
        INNER JOIN Teams at
            ON m.AwayTeamId = at.Id
        WHERE m.IsFinished = 1
        AND (
	        (m.HomeTeamId = @TeamOneId OR m.AwayTeamId = @TeamOneId)
	        AND (m.HomeTeamId=@TeamTwoId OR m.AwayTeamId=@TeamTwoId))";
}
