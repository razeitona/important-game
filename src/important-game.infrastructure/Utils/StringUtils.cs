using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace important_game.infrastructure.Utils;
public static partial class StringUtils
{
    public static string NormalizeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Convert to lowercase
        string normalized = input.ToLowerInvariant().Trim();

        // Remove diacritics/accents
        normalized = RemoveDiacritics(normalized);

        // Remove punctuation and special characters
        normalized = PontuactionRegex().Replace(normalized, "");

        // Replace multiple spaces with a single space
        normalized = MultipleSpacesRegex().Replace(normalized, " ");

        return normalized;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex PontuactionRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpacesRegex();
}
