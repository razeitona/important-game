using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Extensions;
using important_game.infrastructure.Services;
using System.Text;
using System.Xml;

namespace important_game.web.Services;

/// <summary>
/// StringWriter that uses UTF-8 encoding instead of UTF-16
/// </summary>
public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

/// <summary>
/// Generates sitemap.xml and llms.txt files on a cron schedule.
/// Default: Daily at 2:00 AM UTC ("0 0 2 * * *")
/// Configure via appsettings.json: "BackgroundJobs:SitemapGenerator:CronExpression"
/// </summary>
public class SitemapGeneratorService : CronBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public SitemapGeneratorService(
        IServiceProvider serviceProvider,
        ILogger<CronBackgroundService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
        : base(
            cronExpression: configuration["BackgroundJobs:SitemapGenerator:CronExpression"] ?? "0 0 2 * * *",
            logger: logger,
            timeZone: TimeZoneInfo.Utc)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _environment = environment;
    }

    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        var baseUrl = _configuration["SiteSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("BaseUrl is not configured in SiteSettings:BaseUrl");
        }

        // Ensure base URL ends without trailing slash
        baseUrl = baseUrl.TrimEnd('/');

        var sitemapPath = Path.Combine(_environment.WebRootPath, "sitemap.xml");
        var llmsTxtPath = Path.Combine(_environment.WebRootPath, "llms.txt");

        using var scope = _serviceProvider.CreateScope();
        var matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();

        // Get all unfinished matches
        var matches = await matchService.GetAllUnfinishedMatchesAsync(stoppingToken);

        // Filter to only include future matches (not past live matches)
        var now = DateTime.UtcNow;
        matches = matches.Where(m => m.MatchDateUTC > now.AddMinutes(-120)).ToList();

        // Generate sitemap.xml
        var sitemap = GenerateSitemapXml(baseUrl, matches);
        // Use UTF-8 with BOM for better compatibility
        var utf8WithBom = new UTF8Encoding(true);
        await File.WriteAllTextAsync(sitemapPath, sitemap, utf8WithBom, stoppingToken);

        // Generate llms.txt
        var llmsTxt = GenerateLlmsTxt(baseUrl);
        // Use UTF-8 with BOM for better compatibility
        await File.WriteAllTextAsync(llmsTxtPath, llmsTxt, utf8WithBom, stoppingToken);
    }

    private string GenerateSitemapXml(string baseUrl, List<important_game.infrastructure.Contexts.Matches.Data.Entities.MatchDto> matches)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using var stringWriter = new Utf8StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

        // Add static pages
        AddUrl(xmlWriter, baseUrl, "/", priority: 1.0, changeFreq: "daily");
        AddUrl(xmlWriter, baseUrl, "/matches", priority: 0.9, changeFreq: "hourly");
        AddUrl(xmlWriter, baseUrl, "/calendar", priority: 0.8, changeFreq: "daily");
        AddUrl(xmlWriter, baseUrl, "/tvlistings", priority: 0.8, changeFreq: "hourly");
        AddUrl(xmlWriter, baseUrl, "/about", priority: 0.5, changeFreq: "weekly");
        AddUrl(xmlWriter, baseUrl, "/favorites", priority: 0.6, changeFreq: "daily");

        // Add match detail pages
        foreach (var match in matches)
        {
            var homeSlug = SlugHelper.GenerateSlug(match.HomeTeamName);
            var awaySlug = SlugHelper.GenerateSlug(match.AwayTeamName);
            var matchUrl = $"/match/{homeSlug}-vs-{awaySlug}";

            // Use match date as lastmod
            var lastMod = match.MatchDateUTC.ToString("yyyy-MM-dd");

            AddUrl(xmlWriter, baseUrl, matchUrl, priority: 0.8, changeFreq: "daily", lastMod: lastMod);
        }

        xmlWriter.WriteEndElement(); // urlset
        xmlWriter.WriteEndDocument();
        xmlWriter.Flush();

        return stringWriter.ToString();
    }

    private void AddUrl(XmlWriter xmlWriter, string baseUrl, string path, double priority, string changeFreq, string? lastMod = null)
    {
        xmlWriter.WriteStartElement("url");

        xmlWriter.WriteStartElement("loc");
        xmlWriter.WriteString($"{baseUrl}{path}");
        xmlWriter.WriteEndElement(); // loc

        if (!string.IsNullOrWhiteSpace(lastMod))
        {
            xmlWriter.WriteStartElement("lastmod");
            xmlWriter.WriteString(lastMod);
            xmlWriter.WriteEndElement(); // lastmod
        }

        xmlWriter.WriteStartElement("changefreq");
        xmlWriter.WriteString(changeFreq);
        xmlWriter.WriteEndElement(); // changefreq

        xmlWriter.WriteStartElement("priority");
        xmlWriter.WriteString(priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
        xmlWriter.WriteEndElement(); // priority

        xmlWriter.WriteEndElement(); // url
    }

    private string GenerateLlmsTxt(string baseUrl)
    {
        var sb = new StringBuilder();

        // H1 heading (required)
        sb.AppendLine("# Match to Watch");
        sb.AppendLine();

        // Blockquote summary (optional)
        sb.AppendLine("> Match to Watch helps football fans discover the most exciting matches worth watching.");
        sb.AppendLine("> We analyze hundreds of matches weekly using our Excitement Score (ES) algorithm that considers");
        sb.AppendLine("> historical rivalries, team form, league standings, and fixture importance to predict match excitement.");
        sb.AppendLine();

        // Body content
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("Match to Watch is a platform dedicated to helping football enthusiasts identify the most thrilling matches");
        sb.AppendLine("to watch from the hundreds of games played each week. Our unique Excitement Score (ES) system analyzes");
        sb.AppendLine("various factors to predict how exciting a match will be, ranging from 0 to 100.");
        sb.AppendLine();

        sb.AppendLine("### How It Works");
        sb.AppendLine();
        sb.AppendLine("The Excitement Score algorithm considers multiple factors:");
        sb.AppendLine();
        sb.AppendLine("- **Historical Rivalries**: Classic rivalries bring extra intensity");
        sb.AppendLine("- **Form & Momentum**: Recent team performance and playing style");
        sb.AppendLine("- **League Standings**: Title races, relegation battles, top-4 fights");
        sb.AppendLine("- **Head-to-Head History**: Past encounters and competitiveness patterns");
        sb.AppendLine("- **Fixture Importance**: Cup finals, derbies, crucial seasonal matches");
        sb.AppendLine("- **League Quality**: Top-tier competitions feature higher quality football");
        sb.AppendLine();

        sb.AppendLine("We also track matches in real-time, updating excitement scores based on live events such as goals,");
        sb.AppendLine("cards, and possession battles. This means users can discover thrilling matches even after they've kicked off.");
        sb.AppendLine();

        // Pages section
        sb.AppendLine("## Pages");
        sb.AppendLine();
        sb.AppendLine($"- [Home]({baseUrl}/): Main dashboard showing live matches, trending matches, and upcoming fixtures");
        sb.AppendLine($"- [Matches]({baseUrl}/matches): Browse all upcoming matches sorted by excitement score and competition");
        sb.AppendLine($"- [Calendar]({baseUrl}/calendar): Monthly calendar view of all matches with excitement indicators");
        sb.AppendLine($"- [TV Listings]({baseUrl}/tvlistings): Browse upcoming matches by TV broadcast schedule and channels");
        sb.AppendLine($"- [About]({baseUrl}/about): Detailed information about our mission and how the Excitement Score works");
        sb.AppendLine($"- [Favorites]({baseUrl}/favorites): Personalized page for authenticated users to track their favorite matches");
        sb.AppendLine($"- [Settings]({baseUrl}/settings): User account settings including favorite channels and account management");
        sb.AppendLine();

        // Features section
        sb.AppendLine("## Features");
        sb.AppendLine();
        sb.AppendLine("- **Excitement Score (ES)**: 0-100 score predicting match excitement");
        sb.AppendLine("- **Live Match Tracking**: Real-time updates for ongoing matches");
        sb.AppendLine("- **Multi-Competition Coverage**: Tracks top football leagues worldwide");
        sb.AppendLine("- **Match Details**: Comprehensive analysis including head-to-head records, team form, and rivalry information");
        sb.AppendLine("- **TV Broadcast Information**: Find where to watch matches on TV across different countries and channels");
        sb.AppendLine("- **Favorite Channels**: Personalize your TV listings by selecting favorite broadcast channels");
        sb.AppendLine("- **Favorites System**: Users can save matches for quick access (requires Google authentication)");
        sb.AppendLine("- **Responsive Design**: Optimized for desktop and mobile viewing");
        sb.AppendLine();

        return sb.ToString();
    }
}
