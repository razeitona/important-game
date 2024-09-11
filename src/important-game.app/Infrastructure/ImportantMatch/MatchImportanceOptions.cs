using important_game.ui.Core;

namespace important_game.ui.Infrastructure.ImportantMatch
{
    //TODO: Move this to a config file
    public class MatchImportanceOptions
    {

        public Dictionary<int, MatchImportanceLeague> Leagues = new Dictionary<int, MatchImportanceLeague>
        {
            { 0, new MatchImportanceLeague(0, "", 0.5d, "#3c1c5a", "#ffffff") },
            {7, new MatchImportanceLeague(7, "Champions League", 1d, "#3c1c5a", "#ffffff")},
            {8, new MatchImportanceLeague(8, "Espanha", 0.95d, "#ffffff", "#2f4a89") },
            {17, new MatchImportanceLeague(17, "Inglaterra", 0.98d, "#3c1c5a", "#ffffff")},
            {35, new MatchImportanceLeague(35, "Alemanha", 0.95d, "#ffffff", "#e2080e")},
            {23, new MatchImportanceLeague(23, "Itália", 0.92d, "#ffffff", "#09519e")},
            {34, new MatchImportanceLeague(34, "França", 0.90d, "#3c1c5a", "#ffffff")},
            {238, new MatchImportanceLeague(238, "Portugal", 0.90d, "#001841", "#ffc501")},
            {325, new MatchImportanceLeague(325, "Brasil", 0.88d, "#141528", "#C7FF00")},
            {18, new MatchImportanceLeague(18, "Inglaterra - Championship", 0.80d, "#3c1c5a", "#ffffff")},
            {955, new MatchImportanceLeague(955, "Saudi Arabia League", 0.68d, "#ffffff", "#2c9146")},
            {37, new MatchImportanceLeague(37, "Netherlands", 0.87d, "#122e62", "#ffffff")},
            {52, new MatchImportanceLeague(52, "Turkey", 0.85d, "#f00515", "#ffffff")},
            {36, new MatchImportanceLeague(36, "Scotland", 0.80d, "#311b77", "#ffffff")},
            {155, new MatchImportanceLeague(155, "Argentina", 0.75d, "#004a79", "#ffffff")},
        };

        //https://www.soccer-rating.com/Portugal/

        public bool IsValid()
        {
            return Leagues != null && Leagues.Count > 0;
        }

    }
}
