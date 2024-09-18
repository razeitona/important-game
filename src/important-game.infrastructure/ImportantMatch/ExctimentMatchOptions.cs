using important_game.infrastructure.ImportantMatch.Models;
using System.ComponentModel;

namespace important_game.infrastructure.ImportantMatch
{
    //TODO: Move this to a config file
    public class ExctimentMatchOptions
    {

        public List<MatchImportanceLeague> Leagues = new List<MatchImportanceLeague>
        {
            new MatchImportanceLeague(7, "Champions League", 1d, "#3c1c5a", "ffffff"),
            new MatchImportanceLeague(17, "Inglaterra", 0.95d, "#3c1c5a", "#ffffff"),
            new MatchImportanceLeague(35, "Alemanha", 0.85d, "#ffffff", "#e2080e"),
            new MatchImportanceLeague(8, "Espanha", 0.85d, "#ffffff", "#2f4a89"),
            new MatchImportanceLeague(23, "Itália", 0.80d, "#ffffff", "#09519e"),
            new MatchImportanceLeague(34, "França", 0.75d, "#3c1c5a", "#ffffff"),
            new MatchImportanceLeague(37, "Netherlands", 0.70d, "#122e62", "#ffffff"),
            new MatchImportanceLeague(325, "Brasil", 0.70d, "#141528", "#C7FF00"),
            new MatchImportanceLeague(155, "Argentina", 0.65d, "#004a79", "#ffffff"),
            new MatchImportanceLeague(52, "Turkey", 0.65d, "#f00515", "#ffffff"),
            new MatchImportanceLeague(238, "Portugal", 0.60d, "#001841", "#ffc501"),
            new MatchImportanceLeague(18, "Inglaterra - Championship", 0.55d, "#3c1c5a", "#ffffff"),
            new MatchImportanceLeague(36, "Scotland", 0.55d, "#311b77", "#ffffff"),
            new MatchImportanceLeague(955, "Saudi Arabia League", 0.40d, "#ffffff", "#2c9146"),
            new MatchImportanceLeague(11621, "Liga MX", 0.65d, "#3c1c5a", "#ffffff"),
            new MatchImportanceLeague(185, "Greece", 0.65d, "#3c1c5a", "#ffffff"),
        };

        //https://www.soccer-rating.com/Portugal/

        public bool IsValid()
        {
            return Leagues != null && Leagues.Count > 0;
        }

        public static List<RivarlyMatchup> Rivalry = new List<RivarlyMatchup>()
        {
            new RivarlyMatchup {TeamOneId = 42, TeamTwoId = 2672, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 2817, TeamTwoId = 2672, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 2817, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 2829, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId =2218, TeamTwoId =2404, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 2032, TeamTwoId = 5149, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 17, TeamTwoId = 2829, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 17, TeamTwoId = 2672, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 1644, TeamTwoId = 2817, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2672, TeamTwoId = 2829, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 2673, TeamTwoId = 2829, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 2687, TeamTwoId = 2829, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 44, TeamTwoId = 2829, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 44, TeamTwoId = 2687, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2697, TeamTwoId = 2817, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 3006, TeamTwoId = 3002, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 3006, TeamTwoId = 3001, Excitment = 1.0},
            new RivarlyMatchup{TeamOneId = 3002, TeamTwoId = 3001, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2995, TeamTwoId = 3002, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 3009, TeamTwoId = 2999, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 1648, TeamTwoId = 1643, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 1647, TeamTwoId = 1684, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 1649, TeamTwoId = 1678, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 1653, TeamTwoId = 1661, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 1641, TeamTwoId = 1644, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 1649, TeamTwoId = 1641, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 1641, TeamTwoId = 1678, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 1647, TeamTwoId = 1678, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 2672, TeamTwoId = 2677, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 2672, TeamTwoId = 2673, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 2672, TeamTwoId = 2534, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 2524, TeamTwoId = 2534, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 2538, TeamTwoId = 2677, Excitment = 0.55},
            new RivarlyMatchup{TeamOneId = 2697, TeamTwoId = 2692, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 2699, TeamTwoId = 2702, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 2687, TeamTwoId = 2696, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2697, TeamTwoId = 2687, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId =2714, TeamTwoId = 2702, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2685, TeamTwoId = 2693, Excitment = 0.65},
            new RivarlyMatchup{TeamOneId = 2685, TeamTwoId = 2690, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 2693, TeamTwoId = 2687, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2687, TeamTwoId = 2692, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 2687, TeamTwoId = 2714, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2687, TeamTwoId = 2702, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2693, TeamTwoId = 2705, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 2693, TeamTwoId = 2699, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 2693, TeamTwoId = 2702, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 2953, TeamTwoId =2959, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 2953, TeamTwoId = 2952, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2952, TeamTwoId = 2959, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 2953, TeamTwoId = 2950, Excitment = 0.65},
            new RivarlyMatchup{TeamOneId = 2352, TeamTwoId = 2351, Excitment = 1.0},
            new RivarlyMatchup{TeamOneId = 2829, TeamTwoId = 2817, Excitment = 1.0},
            new RivarlyMatchup{TeamOneId = 2829, TeamTwoId = 2825, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 2817, TeamTwoId = 2836, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2829, TeamTwoId = 2836, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 2836, TeamTwoId = 2825, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 2836, TeamTwoId = 2833, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 2825, TeamTwoId = 2824, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2828, TeamTwoId =2819, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 2817, TeamTwoId = 2814, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 2816, TeamTwoId = 2833, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 2859, TeamTwoId = 2845, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 17, TeamTwoId = 42, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 17, TeamTwoId = 44, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 17, TeamTwoId = 38, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 17, TeamTwoId = 35, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 17, TeamTwoId = 33, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 42, TeamTwoId = 44, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 42, TeamTwoId = 40, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 42, TeamTwoId = 38, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 42, TeamTwoId = 35, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 42, TeamTwoId = 33, Excitment = 1.0},
            new RivarlyMatchup{TeamOneId = 42, TeamTwoId = 37, Excitment = 0.8},
            new RivarlyMatchup{TeamOneId = 39, TeamTwoId = 35, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 44, TeamTwoId = 38, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 44, TeamTwoId = 35, Excitment = 1.0},
            new RivarlyMatchup{TeamOneId = 44, TeamTwoId = 48, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 40, TeamTwoId = 38, Excitment = 0.75},
            new RivarlyMatchup{TeamOneId = 40, TeamTwoId = 37, Excitment = 0.65},
            new RivarlyMatchup{TeamOneId = 40, TeamTwoId = 3, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 30, TeamTwoId = 7, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 14, TeamTwoId = 38, Excitment = 0.65},
            new RivarlyMatchup{TeamOneId = 14, TeamTwoId = 31, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 50, Excitment = 0.65},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 35, Excitment = 0.9},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 43, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 33, Excitment = 0.95},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 37, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 38, TeamTwoId = 7, Excitment = 0.7},
            new RivarlyMatchup{TeamOneId = 60, TeamTwoId = 45, Excitment = 0.6},
            new RivarlyMatchup{TeamOneId = 33, TeamTwoId = 37, Excitment = 0.85},
            new RivarlyMatchup{TeamOneId = 33, TeamTwoId = 31, Excitment = 0.7}
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
        public double Excitment { get; set; }
    }
}
