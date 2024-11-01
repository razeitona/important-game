namespace important_game.infrastructure.ImportantMatch.Models.Processors
{
    public class MatchImportanceLeague
    {
        public MatchImportanceLeague(int leagueId, string name, double importance, string primaryColor, string backgroundColor)
        {
            LeagueId = leagueId;
            Name = name;
            Importance = importance;
            PrimaryColor = primaryColor;
            BackgroundColor = backgroundColor;
        }

        public int LeagueId { get; private set; }
        public string Name { get; private set; }
        public double Importance { get; private set; }
        public string PrimaryColor { get; private set; }
        public string BackgroundColor { get; private set; }

        public void UpdateLeagueName(string name)
        {
            Name = name;
        }
    }

}
