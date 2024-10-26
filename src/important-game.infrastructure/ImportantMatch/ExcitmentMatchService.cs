using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExcitmentMatchService(IExctimentMatchRepository matchRepository
        , IExcitmentMatchProcessor matchProcessor) : IExcitmentMatchService
    {

        public async Task<List<ExcitementMatch>> GetAllMatchesAsync()
        {
            var matches = await matchRepository.GetAllMatchesAsync();

            return matches;
        }

        public async Task<ExcitementMatch> GetMatchByIdAsync(int id)
        {
            var match = await matchRepository.GetMatchByIdAsync(id);

            return match;
        }

        public async Task CalculateUpcomingMatchsExcitment()
        {
            var excitementMatches = await matchProcessor.GetUpcomingExcitementMatchesAsync(new ExctimentMatchOptions());

            var currentMatches = await GetAllMatchesAsync();

            var validMatches = excitementMatches.Select(c => c.Id).ToHashSet();

            var liveGames = currentMatches
                  .Where(c => c.MatchDate < DateTime.UtcNow && c.MatchDate > DateTime.UtcNow.AddMinutes(-110))
                  .Where(c => !validMatches.Contains(c.Id))
                  .ToList();

            excitementMatches.AddRange(liveGames);

            await matchRepository.SaveMatchesAsync(excitementMatches);
        }

    }
}
