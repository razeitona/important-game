namespace important_game.infrastructure.ImportantMatch.Models
{
    public class TeamMatchDetailDto : TeamDto
    {
        public List<MatchResultType> Form { get; set; } = new List<MatchResultType>();
        public bool IsTitleHolder { get; internal set; }
    }


}
