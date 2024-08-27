using important_game.ui.Core;

namespace important_game.ui.Infrastructure.ImportantMatch
{
    //TODO: Move this to a config file
    public class MatchImportanceOptions
    {

        public List<MatchImportanceLeague> Leagues = new List<MatchImportanceLeague>
        {
            new MatchImportanceLeague(7, "Champions League", 1d),
            new MatchImportanceLeague(17, "Inglaterra", 0.98d),
            new MatchImportanceLeague(8, "Espanha", 0.95d),
            new MatchImportanceLeague(35, "Alemanha", 0.95d),
            new MatchImportanceLeague(23, "Itália", 0.92d),
            new MatchImportanceLeague(34, "França", 0.90d),
            new MatchImportanceLeague(238, "Portugal", 0.90d),
            new MatchImportanceLeague(325, "Brasil", 0.88d),
            new MatchImportanceLeague(18, "Inglaterra - Championship", 0.80d),

        };

        //https://www.soccer-rating.com/Portugal/

        public bool IsValid()
        {
            return Leagues != null && Leagues.Count > 0;
        }

    }
}
