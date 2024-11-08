using System.ComponentModel;

namespace important_game.infrastructure.ImportantMatch.Models
{
    public enum MatchDataPointInfo
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

}
