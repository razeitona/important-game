using important_game.ui.Core;

namespace important_game.ui.Infrastructure
{
    //TODO: Move this to a config file
    internal static class MatchImportanceOptions
    {

        public static List<SofaScoreLeagueOption> Leagues = new List<SofaScoreLeagueOption>
        {
            new SofaScoreLeagueOption(238, "Portugal", 0.8d),
            new SofaScoreLeagueOption(17, "Inglaterra", 0.98d),
            new SofaScoreLeagueOption(8, "Espanha", 0.9d),
            new SofaScoreLeagueOption(34, "França", 0.81d),
            new SofaScoreLeagueOption(23, "Itália", 0.88d),
            new SofaScoreLeagueOption(35, "Alemanha", 0.9d)
        };

    }
}
