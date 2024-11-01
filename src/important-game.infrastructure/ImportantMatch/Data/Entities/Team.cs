namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int H2hWins { get; set; }
        public int Goals { get; set; }
        public TeamFixtureData LastFixtures { get; set; }
        public bool IsTitleHolder { get; internal set; }
    }
}
