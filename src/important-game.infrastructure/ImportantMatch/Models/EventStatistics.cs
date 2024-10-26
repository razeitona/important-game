
namespace important_game.infrastructure.ImportantMatch.Models
{
    public class EventStatistics
    {
        //Period, Group, Stat Name
        public Dictionary<string, Dictionary<string, Dictionary<string, StatisticsItem>>> Statistics { get; set; } = new();

        public bool GetHomeStatValue(string period, string group, string statKey, out double value)
        {
            value = 0;

            var stat = GetStatValue(period, group, statKey);

            if (stat == null)
                return false;

            value = stat.HomeValue;
            return true;
        }
        public bool GetAwayStatValue(string period, string group, string statKey, out double value)
        {
            value = 0;

            var stat = GetStatValue(period, group, statKey);

            if (stat == null)
                return false;

            value = stat.AwayValue;
            return true;
        }

        private StatisticsItem GetStatValue(string period, string group, string statKey)
        {
            if (Statistics.ContainsKey(period))
            {
                if (Statistics[period].ContainsKey(group))
                {
                    if (Statistics[period][group].ContainsKey(statKey))
                    {
                        return Statistics[period][group][statKey];
                    }
                    return null;
                }
            }

            return null;
        }
    }

    public record StatisticsItem(string Key, string Name, string HomeStat, double HomeValue
        , double? HomeTotal, string AwayStat, double AwayValue, double? AwaytTotal, int CompareCode)
    {
        //public string Name { get; set; }
        ////public string Key { get; set; }
        //public string HomeStat { get; set; }
        //public double HomeValue { get; set; }
        //public double? HomeTotal { get; set; }
        //public string AwayStat { get; set; }
        //public double AwayValue { get; set; }
        //public double? AwayTotal { get; set; }
        //public int CompareCode { get; set; }
        //public string StatisticsType { get; set; }
        //public string ValueType { get; set; }
        //public int RenderType { get; set; }
    }
}
