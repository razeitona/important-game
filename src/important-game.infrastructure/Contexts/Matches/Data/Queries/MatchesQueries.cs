namespace important_game.infrastructure.Contexts.Matches.Data.Queries;
internal static class MatchesQueries
{
    internal const string CheckMatchExists = @"
        SELECT 
            MatchId
        FROM 
            Matches
        WHERE 
            HomeTeamId = @HomeTeamId
            AND AwayTeamId = @AwayTeamId
            AND MatchDateUTC = @MatchDateUTC;";

    internal const string InsertMatch = @"
        INSERT INTO Matches (
            CompetitionId, SeasonId, Round, MatchDateUTC, HomeTeamId, AwayTeamId, 
            HomeScore, AwayScore, IsFinished, UpdatedDateUTC
        ) VALUES (
            @CompetitionId, @SeasonId, @Round, @MatchDateUTC, @HomeTeamId, @AwayTeamId, 
            @HomeScore, @AwayScore, @IsFinished, @UpdatedDateUTC
        );
        SELECT last_insert_rowid();";

    internal const string UpdateMatch = @"
            UPDATE Matches 
            SET CompetitionId = @CompetitionId,
                SeasonId = @SeasonId,
                Round = @Round, 
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
                HomeTeamPosition = @HomeTeamPosition,
                AwayForm = @AwayForm,
                AwayTeamPosition = @AwayTeamPosition,
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

    internal const string SelectAllUnfinishedMatches = @"
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
        INNER JOIN Competitions c
	        ON c.CompetitionId = m.CompetitionId
        WHERE 
            m.IsFinished = 0
        ORDER BY datetime(m.MatchDateUTC) ASC";


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

    internal const string SelectMatchByTeamSlugs = @"
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
        WHERE ht.SlugName = @HomeSlug
          AND at.SlugName = @AwaySlug
          AND m.IsFinished = 0
        ORDER BY m.MatchDateUTC ASC
        LIMIT 1";

    internal const string SelectTeamLatestMatchDate = @"
        SELECT 
	        dateTime(MatchDateUTC) 
        FROM Matches 
        WHERE 
            (HomeTeamId = @TeamId OR AwayTeamId = @TeamId)
            AND IsFinished = 1
        ORDER BY datetime(MatchDateUTC) DESC
        LIMIT 1";

    internal const string SelectRecentMatchesForTeam = @"
        SELECT 
	        *
        FROM Matches 
        WHERE 
            (HomeTeamId = @TeamId OR AwayTeamId = @TeamId)
            AND IsFinished = 1
        ORDER BY datetime(MatchDateUTC) DESC
        LIMIT @NumberOfMatches";



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

    internal const string CheckRecentFinishedMatch = @"
        SELECT 
            COUNT(1)
        FROM Matches
        WHERE 
            CompetitionId = @CompetitionId
            AND SeasonId = @SeasonId
            AND IsFinished = 1  
            AND datetime(MatchDateUTC) > @DateTimeUTC;";

    internal const string UserFavoriteUpcomingMatches = @"
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
        INNER JOIN Competitions c
            ON c.CompetitionId = m.CompetitionId
        INNER JOIN UserFavoriteTeams uft
	        ON uft.TeamId = ht.Id OR uft.TeamId = at.Id
        WHERE 
            m.IsFinished = 0
	        AND uft.UserId = @UserId
        ORDER BY datetime(m.MatchDateUTC) ASC;";

    internal const string SelectMatchesInTimeRange = @"
        SELECT
            m.MatchId,
            m.MatchDateUTC,
            ht.Name as 'HomeTeamName',
            at.Name as 'AwayTeamName'
        FROM Matches m
        INNER JOIN Teams ht
            ON m.HomeTeamId = ht.Id
        INNER JOIN Teams at
            ON m.AwayTeamId = at.Id
        WHERE
            datetime(m.MatchDateUTC) >= datetime(@MinDateUTC);";

    internal const string SelectMatchOfTheDay = @"
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
        WHERE m.IsFinished = 0
          AND datetime(m.MatchDateUTC) >= datetime('now')
          AND datetime(m.MatchDateUTC) <= datetime('now', '+24 hours')
        ORDER BY m.ExcitmentScore DESC
        LIMIT 1";
}
