namespace important_game.ui.Core.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int H2hWins { get; set; }
        public int Goals { get; set; }
        public TeamFixtureData LastFixtures { get; set; }
    }
}
