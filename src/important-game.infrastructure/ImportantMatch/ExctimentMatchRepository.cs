using important_game.infrastructure.ImportantMatch.Models;
using System.Text.Json;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExctimentMatchRepository : IExctimentMatchRepository
    {
        public async Task<List<ExcitementMatch>> GetAllMatchesAsync()
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

        public async Task<ExcitementMatch> GetMatchByIdAsync(int id)
        {
            if (System.IO.File.Exists("data.json"))
            {
                var rawData = await System.IO.File.ReadAllTextAsync("data.json");
                if (!string.IsNullOrWhiteSpace(rawData))
                {
                    var allMatches = JsonSerializer.Deserialize<List<ExcitementMatch>>(rawData);
                    return allMatches.FirstOrDefault(c => c.Id == id);
                }
            }

            return null;
        }

        public async Task SaveMatchesAsync(List<ExcitementMatch> excitementMatches)
        {
            if (excitementMatches == null)
                return;

            await System.IO.File.WriteAllTextAsync("data.json", JsonSerializer.Serialize(excitementMatches));
        }
    }
}
