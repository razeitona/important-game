using important_game.ui.Core;

namespace important_game.ui.Infrastructure.ImportantMatch
{
    //TODO: Move this to a config file
    public class MatchImportanceOptions
    {

        public List<MatchImportanceLeague> Leagues = new List<MatchImportanceLeague>
        {
            //new MatchImportanceLeague(7, "Champions League", 1d, "", ""),
            new MatchImportanceLeague(17, "Inglaterra", 0.98d, "#3c1c5a", "#fffafa"),
            new MatchImportanceLeague(8, "Espanha", 0.95d, "#fffafa", "#2f4a89"),
            new MatchImportanceLeague(35, "Alemanha", 0.95d, "#fffafa", "#e2080e"),
            new MatchImportanceLeague(23, "Itália", 0.92d, "#fffafa", "#09519e"),
            new MatchImportanceLeague(34, "França", 0.90d, "#3c1c5a", "#fffafa"),
            new MatchImportanceLeague(238, "Portugal", 0.90d, "#001841", "#ffc501"),
            new MatchImportanceLeague(325, "Brasil", 0.88d, "#141528", "#C7FF00"),
            new MatchImportanceLeague(18, "Inglaterra - Championship", 0.80d, "#3c1c5a", "#fffafa"),
            new MatchImportanceLeague(955, "Saudi Arabia League", 0.68d, "#fffafa", "#2c9146"),
            new MatchImportanceLeague(37, "Netherlands", 0.87d, "#122e62", "#fffafa"),
            new MatchImportanceLeague(52, "Turkey", 0.85d, "#f00515", "#fffafa"),
            new MatchImportanceLeague(36, "Scotland", 0.80d, "#311b77", "#fffafa"),
            new MatchImportanceLeague(155, "Argentina", 0.75d, "#004a79", "#fffafa"),



        };

        //https://www.soccer-rating.com/Portugal/

        public bool IsValid()
        {
            return Leagues != null && Leagues.Count > 0;
        }

    }
}
