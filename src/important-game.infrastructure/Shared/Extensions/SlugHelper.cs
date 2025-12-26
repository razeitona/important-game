using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace important_game.infrastructure.Shared.Extensions
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Convert to lowercase
            input = input.ToLowerInvariant();

            // Remove accents (normalize)
            input = RemoveDiacritics(input);

            // Replace invalid characters with a hyphen
            input = Regex.Replace(input, @"[^a-z0-9\s-]", "");

            // Replace whitespace and hyphens with a single hyphen
            input = Regex.Replace(input, @"[\s-]+", "-").Trim('-');

            return input;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
