namespace important_game.ui.Core.Models
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public LeagueSeason CurrentSeason { get; set; }
    }

    public class LeagueSeason
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


}
