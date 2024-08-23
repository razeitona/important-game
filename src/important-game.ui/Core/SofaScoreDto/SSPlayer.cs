namespace important_game.ui.Core.SofaScoreDto
{
    public class SSPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SSPlayerSeason> Seasons { get; set; }
        public string TeamName { get; set; }
    }
}
