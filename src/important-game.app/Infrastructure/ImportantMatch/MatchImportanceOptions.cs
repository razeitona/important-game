using important_game.ui.Core;

namespace important_game.ui.Infrastructure.ImportantMatch
{
    //TODO: Move this to a config file
    public class MatchImportanceOptions
    {

        public List<MatchImportanceLeague> Leagues = new List<MatchImportanceLeague>
        {
            new MatchImportanceLeague(238, "Portugal", 0.8d),
            new MatchImportanceLeague(17, "Inglaterra", 0.98d),
            new MatchImportanceLeague(8, "Espanha", 0.9d),
            new MatchImportanceLeague(34, "França", 0.81d),
            new MatchImportanceLeague(23, "Itália", 0.88d),
            new MatchImportanceLeague(35, "Alemanha", 0.9d)
        };

        //https://www.soccer-rating.com/Portugal/

        public bool IsValid()
        {
            return Leagues != null && Leagues.Count > 0;
        }

    }
}
