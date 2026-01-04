namespace important_game.infrastructure.Contexts.BroadcastChannels;

using important_game.infrastructure.Contexts.BroadcastChannels.Helpers;
using important_game.infrastructure.Contexts.BroadcastChannels.Models;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

public interface ITvGuideMatcher
{
    List<BroadcastMatchLinkDto> ProcessGuideAndFindMatchesAsync(string xmlContent, List<MatchBroadcastFinderDto> allFutureMatches);
}

public class TvGuideMatcher : ITvGuideMatcher
{
    private readonly IMatchesRepository _matchRepository;

    // Configurable thresholds
    private const double TeamNameSimilarityThreshold = 0.65; // 65% match required for team names
    private const int TimeBufferMinutes = 15; // Match can start 15 mins before program starts (edge case)

    public TvGuideMatcher(IMatchesRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public List<BroadcastMatchLinkDto> ProcessGuideAndFindMatchesAsync(string xmlContent, List<MatchBroadcastFinderDto> allFutureMatches)
    {
        var links = new List<BroadcastMatchLinkDto>();

        // 1. Parse XML
        var programmes = ParseXmlTv(xmlContent);

        if (programmes.Count == 0)
            return [];

        var matchesGroupedByDate = allFutureMatches
            .GroupBy(m => m.MatchDateUTC.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 4. Iterate and Match
        // Optimization: Group matches by date to avoid iterating all matches for every programme
        foreach (var prog in programmes)
        {
            foreach (var listOfMatches in matchesGroupedByDate.Where(c => c.Key == prog.StartUtc.Date))
            {
                foreach (var match in listOfMatches.Value)
                {
                    // 5. Fuzzy Search Logic
                    var confidence = CalculateMatchConfidence(match, prog);

                    // If we are confident enough, add to list
                    if (confidence > 0)
                    {
                        var channelCode = NormalizeChannelCode(prog.ChannelName);
                        if (links.Any(c => c.MatchId == match.MatchId && c.MappedChannelCode == channelCode))
                            continue;
                        links.Add(new BroadcastMatchLinkDto
                        {
                            MatchId = match.MatchId,
                            ChannelName = prog.ChannelName,
                            MappedChannelCode = channelCode,
                            MatchConfidenceScore = confidence
                        });
                    }
                }
            }
        }

        return links.ToList();
    }

    // ---------------------------------------------------------
    // Parsing Logic
    // ---------------------------------------------------------

    private List<TvProgrammeDto> ParseXmlTv(string xmlContent)
    {
        var result = new List<TvProgrammeDto>();
        var doc = XDocument.Parse(xmlContent);

        // Date format in XMLTV: "20260102001500 +0000"
        string dateFormat = "yyyyMMddHHmmss zzz";
        var now = DateTime.UtcNow;

        Dictionary<string, string> channels = [];

        foreach (var p in doc.Descendants("channel"))
        {
            var channelId = p.Attribute("id")?.Value;
            var channelName = p.Element("display-name")?.Value;

            if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(channelName)) continue;

            channels.TryAdd(channelId, channelName);
        }

        foreach (var p in doc.Descendants("programme"))
        {
            var startStr = p.Attribute("start")?.Value;
            var stopStr = p.Attribute("stop")?.Value;
            var channelId = p.Attribute("channel")?.Value;
            var title = p.Element("title")?.Value;
            var desc = p.Element("desc")?.Value ?? "";


            if (string.IsNullOrEmpty(startStr) || string.IsNullOrEmpty(channelId)) continue;

            if (!channels.TryGetValue(channelId, out string channelName))
                continue;

            if (DateTime.TryParseExact(startStr, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime startUtc) &&
                DateTime.TryParseExact(stopStr, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime endUtc))
            {
                if (endUtc < now)
                    continue;

                result.Add(new TvProgrammeDto
                {
                    ChannelName = channelName,
                    StartUtc = startUtc,
                    EndUtc = endUtc,
                    Title = title,
                    Description = desc
                });
            }
        }
        return result;
    }

    // ---------------------------------------------------------
    // Matching Logic
    // ---------------------------------------------------------

    private double CalculateMatchConfidence(MatchBroadcastFinderDto match, TvProgrammeDto prog)
    {
        // Combine Title and Desc for broader search, but weight Title higher
        string searchText = prog.Title;

        // Check Home Team
        bool homeFound = BroadcastFuzzySearchHelper.ContainsFuzzy(searchText, match.HomeTeamName);

        // Check Away Team
        bool awayFound = BroadcastFuzzySearchHelper.ContainsFuzzy(searchText, match.AwayTeamName);

        if (homeFound && awayFound)
        {
            // Check if it's explicitly marked as Live to boost score
            if (searchText.IndexOf("Live", StringComparison.OrdinalIgnoreCase) >= 0 ||
                searchText.IndexOf("Direto", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 1.0; // Perfect match
            }
            return 0.9; // Very high probability
        }

        // Edge case: Sometimes only one team is mentioned in a specific show context, 
        // but for a "Match Listing" typically both are required. 
        // We return 0 if both aren't found to avoid false positives (e.g. "Benfica News" matching a Benfica game)
        return 0.0;
    }

    private string NormalizeChannelCode(string xmlId)
    {
        // Simple helper to strip .pt@SD, .uk@Portugal etc to match your DB Codes if needed
        // Example: "SportTV1.pt@SD" -> "SportTV1"
        if (string.IsNullOrEmpty(xmlId)) return xmlId;
        var parts = xmlId.Split(new[] { '.', '@' });
        return parts[0].Replace(" ", "").ToUpper(); // Heuristic normalization
    }
}