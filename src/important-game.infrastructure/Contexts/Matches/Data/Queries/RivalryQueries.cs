namespace important_game.infrastructure.Contexts.Matches.Data.Queries;
internal static class RivalryQueries
{
    internal const string SelectRivalryByTeamId = @"
            SELECT 
                TeamOneId,
                TeamTwoId,
                RivarlyValue
            FROM TeamRivalries 
            WHERE (TeamOneId = @TeamOneId AND TeamTwoId = @TeamTwoId)
               OR (TeamTwoId = @TeamOneId AND TeamOneId = @TeamTwoId)
            LIMIT 1";
}
