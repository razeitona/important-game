using important_game.ui.Core.Models;

namespace important_game.ui.Infrastructure.ImportantMatch
{
    public interface IMatchProcessorService
    {
        Task<List<League>> GetLeaguesAsync(params int[] sofaLeaguesIds);
        Task<LeagueUpcomingFixtures> GetUpcomingFixturesAsync(int leagueId, int seasonId);
        Task<MatchImportance> CalculateMatchImportanceAsync(ImportantMatchCalculatorOption options, Fixture fixture);
    }
}
