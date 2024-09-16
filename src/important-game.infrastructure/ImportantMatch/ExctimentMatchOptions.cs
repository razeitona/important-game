using important_game.infrastructure.ImportantMatch.Models;
using System.ComponentModel;
using System.Reflection;

namespace important_game.infrastructure.ImportantMatch
{
    //TODO: Move this to a config file
    public class ExctimentMatchOptions
    {

        public List<MatchImportanceLeague> Leagues = new List<MatchImportanceLeague>
        {
            new MatchImportanceLeague(7, "Champions League", 1d, "#3c1c5a", "ffffff"),
            new MatchImportanceLeague(17, "Inglaterra", 0.98d, "#3c1c5a", "#ffffff"),
            new MatchImportanceLeague(8, "Espanha", 0.95d, "#ffffff", "#2f4a89"),
            new MatchImportanceLeague(35, "Alemanha", 0.95d, "#ffffff", "#e2080e"),
            new MatchImportanceLeague(23, "Itália", 0.89d, "#ffffff", "#09519e"),
            new MatchImportanceLeague(34, "França", 0.75d, "#3c1c5a", "#ffffff"),
            new MatchImportanceLeague(238, "Portugal", 0.70d, "#001841", "#ffc501"),
            new MatchImportanceLeague(325, "Brasil", 0.65d, "#141528", "#C7FF00"),
            new MatchImportanceLeague(18, "Inglaterra - Championship", 0.60d, "#3c1c5a", "#ffffff"),
            new MatchImportanceLeague(955, "Saudi Arabia League", 0.38d, "#ffffff", "#2c9146"),
            new MatchImportanceLeague(37, "Netherlands", 0.70d, "#122e62", "#ffffff"),
            new MatchImportanceLeague(52, "Turkey", 0.65d, "#f00515", "#ffffff"),
            new MatchImportanceLeague(36, "Scotland", 0.60d, "#311b77", "#ffffff"),
            new MatchImportanceLeague(155, "Argentina", 0.55d, "#004a79", "#ffffff"),
        };

        //https://www.soccer-rating.com/Portugal/

        public bool IsValid()
        {
            return Leagues != null && Leagues.Count > 0;
        }

        public static List<RivarlyMatchup> Rivalry = new List<RivarlyMatchup>()
        {
            new RivarlyMatchup{TeamOneId = 42, TeamTwoId = 33, Exctiment = 1d},
            new RivarlyMatchup{TeamOneId = 2692, TeamTwoId = 44, Exctiment = 0.75d},
            new RivarlyMatchup{TeamOneId = 2999, TeamTwoId = 3009, Exctiment = 0.65d}
        };

    }

    public enum MatchDataPoint
    {
        [Description("Competition Rank")] CompetitionRank,
        [Description("Fixture")] FixtureValue,
        [Description("Current Form")] TeamsLastFixtureFormValue,
        [Description("Goals by both teams")] TeamsGoalsFormValue,
        [Description("League Standing")] LeagueTableValue,
        [Description("Head to Head")] H2HValue,
        [Description("Title Holder")] TitleHolderValue,
        [Description("Rivalry")] RivalryValue
    }



    public class RivarlyMatchup
    {
        public int TeamOneId { get; set; }
        public int TeamTwoId { get; set; }
        public double Exctiment { get; set; }
    }
}
