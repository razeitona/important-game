using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExcitmentMatchService(IExctimentMatchRepository matchRepository) : IExcitmentMatchService
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
    }
}
