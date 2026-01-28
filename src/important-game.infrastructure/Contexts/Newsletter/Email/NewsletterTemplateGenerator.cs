using System.Diagnostics.CodeAnalysis;
using System.Text;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using Microsoft.Extensions.Options;

namespace important_game.infrastructure.Contexts.Newsletter.Email;

public interface INewsletterTemplateGenerator
{
    string GenerateWeeklyNewsletter(List<MatchDetailDto> topMatches, string unsubscribeToken);
}

[ExcludeFromCodeCoverage]
public class NewsletterTemplateGenerator : INewsletterTemplateGenerator
{
    private readonly EmailOptions _options;

    public NewsletterTemplateGenerator(IOptions<EmailOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string GenerateWeeklyNewsletter(List<MatchDetailDto> topMatches, string unsubscribeToken)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var sb = new StringBuilder();

        sb.Append(GenerateHtmlHeader());
        sb.Append(GenerateEmailBody(topMatches, baseUrl, unsubscribeToken));
        sb.Append(GenerateHtmlFooter());

        return sb.ToString();
    }

    private static string GenerateHtmlHeader()
    {
        return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <title>Match to Watch - Weekly Newsletter</title>
    <!--[if mso]>
    <noscript>
        <xml>
            <o:OfficeDocumentSettings>
                <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
        </xml>
    </noscript>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #1a1d24; color: #ffffff;"">";
    }

    private static string GenerateHtmlFooter()
    {
        return @"</body>
</html>";
    }

    private string GenerateEmailBody(List<MatchDetailDto> topMatches, string baseUrl, string unsubscribeToken)
    {
        var sb = new StringBuilder();

        // Main container
        sb.Append(@"
<table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #1a1d24;"">
    <tr>
        <td align=""center"" style=""padding: 20px 10px;"">
            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""600"" style=""max-width: 600px; background-color: #21242c; border-radius: 12px; overflow: hidden;"">");

        // Header with logo
        sb.Append($@"
                <tr>
                    <td style=""padding: 30px 30px 20px 30px; text-align: center; background: linear-gradient(135deg, #258cfb 0%, #1a73e8 100%);"">
                        <img src=""{baseUrl}/matchtowatch.svg"" alt=""Match to Watch"" width=""180"" style=""display: block; margin: 0 auto; max-width: 180px;"" />
                        <h1 style=""margin: 20px 0 10px 0; font-size: 24px; font-weight: 700; color: #ffffff;"">Weekly Match Preview</h1>
                        <p style=""margin: 0; font-size: 14px; color: rgba(255,255,255,0.85);"">Your curated selection of the most exciting matches this week</p>
                    </td>
                </tr>");

        // Introduction
        sb.Append(@"
                <tr>
                    <td style=""padding: 25px 30px 15px 30px;"">
                        <p style=""margin: 0; font-size: 15px; line-height: 1.6; color: #b0b3b8;"">
                            Here are the top 5 matches of the upcoming week, ranked by our <strong style=""color: #258cfb;"">Excitement Score</strong> algorithm.
                            This score considers league importance, team form, standings, head-to-head history, and more to predict the most thrilling games.
                        </p>
                    </td>
                </tr>");

        // Matches section
        if (topMatches.Count > 0)
        {
            // Featured match (top 1) with full breakdown
            var featuredMatch = topMatches[0];
            sb.Append(GenerateFeaturedMatchHtml(featuredMatch, baseUrl));

            // Remaining matches (2-5)
            if (topMatches.Count > 1)
            {
                sb.Append(@"
                <tr>
                    <td style=""padding: 10px 30px;"">
                        <h2 style=""margin: 0 0 15px 0; font-size: 18px; font-weight: 600; color: #ffffff; border-bottom: 2px solid #258cfb; padding-bottom: 10px;"">Other Top Matches</h2>
                    </td>
                </tr>");

                for (int i = 1; i < topMatches.Count && i < 5; i++)
                {
                    sb.Append(GenerateMatchCardHtml(topMatches[i], baseUrl, i + 1));
                }
            }
        }

        // See More link
        sb.Append($@"
                <tr>
                    <td style=""padding: 25px 30px; text-align: center;"">
                        <a href=""{baseUrl}/matches"" style=""display: inline-block; padding: 14px 40px; background: linear-gradient(135deg, #258cfb 0%, #1a73e8 100%); color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px;"">
                            See All Matches
                        </a>
                    </td>
                </tr>");

        // Footer
        sb.Append($@"
                <tr>
                    <td style=""padding: 25px 30px; background-color: #1a1d24; border-top: 1px solid #3a3d45;"">
                        <p style=""margin: 0 0 10px 0; font-size: 12px; color: #71777d; text-align: center;"">
                            You're receiving this email because you subscribed to our newsletter at <a href=""{baseUrl}"" style=""color: #258cfb; text-decoration: none;"">matchtowatch.net</a>
                        </p>
                        <p style=""margin: 0; font-size: 12px; color: #71777d; text-align: center;"">
                            <a href=""{baseUrl}/unsubscribe?token={unsubscribeToken}"" style=""color: #258cfb; text-decoration: underline;"">Unsubscribe</a> from this newsletter
                        </p>
                    </td>
                </tr>");

        // Close main tables
        sb.Append(@"
            </table>
        </td>
    </tr>
</table>");

        return sb.ToString();
    }

    private string GenerateFeaturedMatchHtml(MatchDetailDto match, string baseUrl)
    {
        var matchUrl = $"{baseUrl}/match/{GenerateSlug(match.HomeTeamName)}-vs-{GenerateSlug(match.AwayTeamName)}";
        var excitementPercent = (int)(match.ExcitmentScore * 100);
        var matchDate = match.MatchDateUTC.ToString("dddd, MMMM d 'at' HH:mm 'UTC'");

        var sb = new StringBuilder();

        sb.Append($@"
                <tr>
                    <td style=""padding: 15px 30px;"">
                        <div style=""background: linear-gradient(135deg, rgba(37, 140, 251, 0.15) 0%, rgba(37, 140, 251, 0.05) 100%); border: 1px solid rgba(37, 140, 251, 0.3); border-radius: 12px; overflow: hidden;"">
                            <!-- Featured Badge -->
                            <div style=""background: linear-gradient(90deg, #258cfb 0%, #33b5e5 100%); padding: 8px 15px;"">
                                <span style=""font-size: 12px; font-weight: 700; color: #ffffff; text-transform: uppercase; letter-spacing: 1px;"">&#9733; Match of the Week</span>
                            </div>

                            <!-- Match Header -->
                            <div style=""padding: 20px; text-align: center;"">
                                <p style=""margin: 0 0 5px 0; font-size: 13px; color: #71777d;"">{match.CompetitionName}</p>
                                <p style=""margin: 0 0 15px 0; font-size: 12px; color: #b0b3b8;"">{matchDate}</p>

                                <!-- Teams -->
                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                    <tr>
                                        <td width=""40%"" style=""text-align: center; vertical-align: top;"">
                                            <img src=""{baseUrl}/images/team/{match.HomeTeamId}.png"" alt=""{match.HomeTeamName}"" width=""70"" height=""70"" style=""display: block; margin: 0 auto 10px auto;"" />
                                            <p style=""margin: 0; font-size: 15px; font-weight: 600; color: #ffffff;"">{match.HomeTeamName}</p>
                                            {(match.HomeTeamTablePosition.HasValue ? $@"<p style=""margin: 5px 0 0 0; font-size: 12px; color: #71777d;"">{GetOrdinal(match.HomeTeamTablePosition.Value)} in table</p>" : "")}
                                        </td>
                                        <td width=""20%"" style=""text-align: center; vertical-align: middle;"">
                                            <span style=""font-size: 20px; font-weight: 700; color: #71777d;"">VS</span>
                                        </td>
                                        <td width=""40%"" style=""text-align: center; vertical-align: top;"">
                                            <img src=""{baseUrl}/images/team/{match.AwayTeamId}.png"" alt=""{match.AwayTeamName}"" width=""70"" height=""70"" style=""display: block; margin: 0 auto 10px auto;"" />
                                            <p style=""margin: 0; font-size: 15px; font-weight: 600; color: #ffffff;"">{match.AwayTeamName}</p>
                                            {(match.AwayTeamTablePosition.HasValue ? $@"<p style=""margin: 5px 0 0 0; font-size: 12px; color: #71777d;"">{GetOrdinal(match.AwayTeamTablePosition.Value)} in table</p>" : "")}
                                        </td>
                                    </tr>
                                </table>

                                <!-- Excitement Score -->
                                <div style=""margin: 20px 0; padding: 15px; background-color: rgba(0,0,0,0.2); border-radius: 8px;"">
                                    <p style=""margin: 0 0 5px 0; font-size: 12px; color: #71777d; text-transform: uppercase; letter-spacing: 1px;"">Excitement Score</p>
                                    <p style=""margin: 0; font-size: 36px; font-weight: 700; color: {GetScoreColor(match.ExcitmentScore)};"">
                                        &#128293; {excitementPercent}
                                    </p>
                                </div>
                            </div>

                            <!-- Score Breakdown Table -->
                            <div style=""padding: 0 20px 20px 20px;"">
                                <p style=""margin: 0 0 10px 0; font-size: 13px; font-weight: 600; color: #ffffff;"">Why this match is exciting:</p>
                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""font-size: 13px;"">");

        // Score breakdown rows
        sb.Append(GenerateScoreRow("League Importance", match.CompetitionScore));
        sb.Append(GenerateScoreRow("Table Position Impact", match.CompetitionStandingScore));
        sb.Append(GenerateScoreRow("Fixture Importance", match.FixtureScore));
        sb.Append(GenerateScoreRow("Teams Form", match.FormScore));
        sb.Append(GenerateScoreRow("Scoring Potential", match.GoalsScore));
        sb.Append(GenerateScoreRow("Head to Head", match.HeadToHeadScore));

        if (match.RivalryScore > 0)
            sb.Append(GenerateScoreRow("Rivalry Bonus", match.RivalryScore));

        if (match.TitleHolderScore > 0)
            sb.Append(GenerateScoreRow("Title Holder Bonus", match.TitleHolderScore));

        sb.Append($@"
                                </table>
                            </div>

                            <!-- CTA Button -->
                            <div style=""padding: 0 20px 20px 20px; text-align: center;"">
                                <a href=""{matchUrl}"" style=""display: inline-block; padding: 12px 30px; background-color: #258cfb; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 14px;"">
                                    View Match Details
                                </a>
                            </div>
                        </div>
                    </td>
                </tr>");

        return sb.ToString();
    }

    private string GenerateMatchCardHtml(MatchDetailDto match, string baseUrl, int rank)
    {
        var matchUrl = $"{baseUrl}/match/{GenerateSlug(match.HomeTeamName)}-vs-{GenerateSlug(match.AwayTeamName)}";
        var excitementPercent = (int)(match.ExcitmentScore * 100);
        var matchDate = match.MatchDateUTC.ToString("ddd, MMM d 'at' HH:mm 'UTC'");

        return $@"
                <tr>
                    <td style=""padding: 8px 30px;"">
                        <a href=""{matchUrl}"" style=""text-decoration: none; display: block;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #2a2d35; border-radius: 10px; overflow: hidden;"">
                                <tr>
                                    <td style=""padding: 15px;"">
                                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                            <tr>
                                                <!-- Rank -->
                                                <td width=""35"" style=""vertical-align: middle;"">
                                                    <span style=""display: inline-block; width: 28px; height: 28px; line-height: 28px; text-align: center; background-color: #3a3d45; border-radius: 50%; font-size: 13px; font-weight: 600; color: #b0b3b8;"">{rank}</span>
                                                </td>

                                                <!-- Teams -->
                                                <td style=""vertical-align: middle; padding-left: 10px;"">
                                                    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                                        <tr>
                                                            <td style=""vertical-align: middle;"">
                                                                <img src=""{baseUrl}/images/team/{match.HomeTeamId}.png"" alt=""{match.HomeTeamName}"" width=""28"" height=""28"" style=""display: inline-block; vertical-align: middle;"" />
                                                            </td>
                                                            <td style=""vertical-align: middle; padding-left: 8px;"">
                                                                <span style=""font-size: 14px; font-weight: 500; color: #ffffff;"">{match.HomeTeamName}</span>
                                                                {(match.HomeTeamTablePosition.HasValue ? $@"<span style=""font-size: 11px; color: #71777d;""> ({GetOrdinal(match.HomeTeamTablePosition.Value)})</span>" : "")}
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""vertical-align: middle; padding-top: 5px;"">
                                                                <img src=""{baseUrl}/images/team/{match.AwayTeamId}.png"" alt=""{match.AwayTeamName}"" width=""28"" height=""28"" style=""display: inline-block; vertical-align: middle;"" />
                                                            </td>
                                                            <td style=""vertical-align: middle; padding-left: 8px; padding-top: 5px;"">
                                                                <span style=""font-size: 14px; font-weight: 500; color: #ffffff;"">{match.AwayTeamName}</span>
                                                                {(match.AwayTeamTablePosition.HasValue ? $@"<span style=""font-size: 11px; color: #71777d;""> ({GetOrdinal(match.AwayTeamTablePosition.Value)})</span>" : "")}
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>

                                                <!-- Score & Date -->
                                                <td width=""90"" style=""text-align: right; vertical-align: middle;"">
                                                    <p style=""margin: 0; font-size: 22px; font-weight: 700; color: {GetScoreColor(match.ExcitmentScore)};"">
                                                        &#128293; {excitementPercent}
                                                    </p>
                                                    <p style=""margin: 5px 0 0 0; font-size: 11px; color: #71777d;"">{matchDate}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </a>
                    </td>
                </tr>";
    }

    private static string GenerateScoreRow(string label, double score)
    {
        var scorePercent = (int)(score * 100);
        var barWidth = Math.Max(5, scorePercent); // Minimum 5% width for visibility

        return $@"
                                    <tr>
                                        <td style=""padding: 6px 0; color: #b0b3b8;"">{label}</td>
                                        <td width=""120"" style=""padding: 6px 0; text-align: right;"">
                                            <div style=""display: inline-block; width: 80px; height: 8px; background-color: #3a3d45; border-radius: 4px; overflow: hidden; vertical-align: middle;"">
                                                <div style=""width: {barWidth}%; height: 100%; background: linear-gradient(90deg, #258cfb 0%, #33b5e5 100%);""></div>
                                            </div>
                                            <span style=""display: inline-block; width: 30px; text-align: right; color: #ffffff; font-weight: 500;"">{scorePercent}</span>
                                        </td>
                                    </tr>";
    }

    private static string GetScoreColor(double score)
    {
        return score switch
        {
            >= 0.7 => "#33b5e5", // Blue - high excitement
            >= 0.5 => "#00C851", // Green - good excitement
            >= 0.3 => "#ffbb33", // Yellow/orange - moderate
            _ => "#ff4444"       // Red - low
        };
    }

    private static string GetOrdinal(int number)
    {
        if (number <= 0) return number.ToString();

        var suffix = (number % 100) switch
        {
            11 or 12 or 13 => "th",
            _ => (number % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            }
        };

        return $"{number}{suffix}";
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace("'", "")
            .Replace("&", "and");
    }
}
