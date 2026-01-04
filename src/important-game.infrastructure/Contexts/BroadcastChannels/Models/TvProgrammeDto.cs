namespace important_game.infrastructure.Contexts.BroadcastChannels.Models;
public class TvProgrammeDto
{
    public string ChannelName { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}