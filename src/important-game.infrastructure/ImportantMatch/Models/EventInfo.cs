namespace important_game.infrastructure.ImportantMatch.Models
{
    public class EventInfo : Fixture
    {
        public EventStatus Status { get; set; }
    }

    public class EventStatus
    {
        public string Status { get; set; }
        public string? Period { get; set; }
        public long MatchStartTimestamp { get; set; }
        public long MatchPeriodStartTimestamp { get; set; }
        public int? InjuryTime1 { get; set; }
        public int? InjuryTime2 { get; set; }

        public int GetGameTime()
        {
            var matchStartTime = TimeSpan.FromSeconds(MatchStartTimestamp);
            var matchPeriodStartTime = TimeSpan.FromSeconds(MatchPeriodStartTimestamp);

            if (string.IsNullOrWhiteSpace(Period))
            {
                return 90 + (InjuryTime1 ?? 0) + (InjuryTime2 ?? 0);
            }

            if (Period == "period1")
            {
                return (int)(DateTime.UtcNow - matchStartTime).TimeOfDay.TotalMinutes;
            }

            if (Period == "period2")
            {
                return (45 + (InjuryTime1 ?? 0)) + ((int)(DateTime.UtcNow - matchPeriodStartTime).TimeOfDay.TotalMinutes);
            }


            return 0;


        }
    }


}
