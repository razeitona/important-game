namespace important_game.ui.Core
{
    public class SofaScoreLeagueOption
    {
        public SofaScoreLeagueOption(int leagueId, string name, double importance)
        {
            LeagueId = leagueId;
            Name = name;
            Importance = importance;
        }

        public int LeagueId { get; private set; }
        public string Name { get; private set; }
        public double Importance { get; private set; }
    }

   

}
