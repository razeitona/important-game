
namespace important_game.ui.Core
{
    public class MatchImportanceLeague
    {
        public MatchImportanceLeague(int leagueId, string name, double importance)
        {
            LeagueId = leagueId;
            Name = name;
            Importance = importance;
        }

        public int LeagueId { get; private set; }
        public string Name { get; private set; }
        public double Importance { get; private set; }
        public string PrimaryColor { get; private set; }
        public string SecondaryColor { get; private set; }

        public void UpdateLeagueName(string name)
        {
            Name = name;
        }

        internal void UpdateLeagueColors(string primaryColor, string secondaryColor)
        {
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
        }
    }

}
