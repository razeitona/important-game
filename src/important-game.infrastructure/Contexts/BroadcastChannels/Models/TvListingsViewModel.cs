using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.BroadcastChannels.Models;

public class TvListingsViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TvListingDayViewModel> Days { get; set; } = new();
}

public class TvListingDayViewModel
{
    public DateTime Date { get; set; }
    public List<TvListingTimeSlotViewModel> TimeSlots { get; set; } = new();
}

public class TvListingTimeSlotViewModel
{
    public DateTime TimeSlot { get; set; }
    public List<TvListingMatchViewModel> Matches { get; set; } = new();
}

public class TvListingMatchViewModel
{
    public int MatchId { get; set; }
    public int CompetitionId { get; set; }
    public string? CompetitionName { get; set; }
    public DateTime MatchDateUTC { get; set; }
    public int HomeTeamId { get; set; }
    public required string HomeTeamName { get; set; }
    public int AwayTeamId { get; set; }
    public required string AwayTeamName { get; set; }
    public double ExcitmentScore { get; set; }
    public List<MatchBroadcastViewModel> Broadcasts { get; set; } = new();
}
