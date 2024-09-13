using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExcitmentMatchService(IExctimentMatchRepository matchRepository) : IExcitmentMatchService
    {
        public async Task<List<ExcitementMatch>> GetAllMatches()
        {
            var matches = await matchRepository.GetAllMatches();

            return matches;
        }
    }
}
