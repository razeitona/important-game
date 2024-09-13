using important_game.infrastructure.ImportantMatch.Models;
using System.Text.Json;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExctimentMatchRepository : IExctimentMatchRepository
    {
        public async Task<List<ExcitementMatch>> GetAllMatches()
        {
            var matches = new List<ExcitementMatch>();

            if (System.IO.File.Exists("data.json"))
            {
                var rawData = await System.IO.File.ReadAllTextAsync("data.json");
                if (!string.IsNullOrWhiteSpace(rawData))
                {
                    matches = JsonSerializer.Deserialize<List<ExcitementMatch>>(rawData);
                }
            }

            return matches;
        }
    }
}
