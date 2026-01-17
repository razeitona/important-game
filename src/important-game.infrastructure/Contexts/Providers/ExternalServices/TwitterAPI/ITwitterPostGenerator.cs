using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;

public interface ITwitterPostGenerator
{
    string GeneratePreMatchPost(MatchDetailDto match);
}
