using System.Text.RegularExpressions;

namespace important_game.infrastructure.Contexts.BroadcastChannels.Helpers;

public static class BroadcastFuzzySearchHelper
{
    public static bool ContainsFuzzy(string text, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchTerm)) return false;

        // 1. Direct contains (Fastest)
        if (text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) return true;

        // 2. Tokenize and check similarity
        var textTokens = Tokenize(text);
        var searchTokens = Tokenize(searchTerm);

        // Simple heuristic: If the team name has multiple words (e.g. "Man City"), 
        // ensure most parts are present. If single word (e.g. "Benfica"), strict fuzzy.

        int matches = 0;
        foreach (var sToken in searchTokens)
        {
            // Skip very small words like "FC", "SL", "vs"
            if (sToken.Length < 3) continue;

            foreach (var tToken in textTokens)
            {
                if (LevenshteinDistance(sToken, tToken) <= 2) // Allow 1 or 2 character errors depending on length
                {
                    matches++;
                    break;
                }
            }
        }

        // If we found significant parts of the team name
        int significantTokens = searchTokens.Count(t => t.Length >= 3);
        if (significantTokens > 0 && matches >= significantTokens) return true;

        return false;
    }

    private static List<string> Tokenize(string input)
    {
        // Remove special chars and split
        var clean = Regex.Replace(input, @"[^\w\s]", "");
        return clean.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    // Standard Levenshtein implementation
    public static int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}
