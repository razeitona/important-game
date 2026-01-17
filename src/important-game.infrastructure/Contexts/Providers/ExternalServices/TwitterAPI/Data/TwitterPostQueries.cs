namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Data;

internal static class TwitterPostQueries
{
    internal const string InsertTwitterPost = @"
        INSERT INTO TwitterPosts (MatchId, TweetId, Content, PostedDateUTC)
        VALUES (@MatchId, @TweetId, @Content, @PostedDateUTC)";

    internal const string SelectByMatchId = @"
        SELECT Id, MatchId, TweetId, Content, PostedDateUTC
        FROM TwitterPosts
        WHERE MatchId = @MatchId";

    internal const string CountPostsInMonth = @"
        SELECT COUNT(*)
        FROM TwitterPosts
        WHERE PostedDateUTC >= @StartOfMonth AND PostedDateUTC < @EndOfMonth";
}
