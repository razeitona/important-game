namespace important_game.infrastructure.Contexts.Matches.Utils;
internal static class MatchScoreUtil
{

    public static readonly Dictionary<string, string> HighValuePhrases = new()
    {
        { "League Coeficient", "significant impact on league standings" },
        { "League Standings", "high table impact" },
        { "Fixture Importance", "crucial stage in the season" },
        { "Teams Form", "teams recent performances" },
        { "Teams Goals", "high scoring potential" },
        { "Head to head", "compelling head-to-head history" },
        { "Rivalry", "historical rivalry between the teams" },
        { "Title Holder", "presence of the defending champions" },
        { "Score Line", "tight match" },
        { "xGoals", "high value of xGoals" },
        { "Fouls", "few stoppages" },
        { "Cards", "cards with high impact in the final result" },
        { "Possession", "teams fighting for the win" },
        { "Big chances", "amount of big chances" }
    };
}
