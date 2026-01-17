using important_game.infrastructure.Contexts.Matches.Data.Entities;
using Microsoft.Extensions.Logging;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;

public class TwitterPostGenerator(ILogger<TwitterPostGenerator> logger) : ITwitterPostGenerator
{
    private readonly ILogger<TwitterPostGenerator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private static readonly Random _random = new();

    public string GeneratePreMatchPost(MatchDetailDto match)
    {
        var timeUntilMatch = match.MatchDateUTC - DateTimeOffset.UtcNow;
        var hoursUntilMatch = (int)Math.Ceiling(timeUntilMatch.TotalHours);
        var timeText = GetTimeText(hoursUntilMatch);

        var template = SelectTemplate(match);
        var post = FormatTemplate(template, match, timeText);

        // Add hashtag only if there's enough space
        var hashtag = GenerateHashtag(match);
        if (!string.IsNullOrEmpty(hashtag) && post.Length + hashtag.Length + 1 <= 280)
        {
            post = $"{post} {hashtag}";
        }

        if (post.Length > 280)
        {
            post = post[..277] + "...";
        }

        _logger.LogDebug("Generated post for match {MatchId}: {Post}", match.MatchId, post);

        return post;
    }

    private static string GetTimeText(int hoursUntilMatch)
    {
        if (hoursUntilMatch <= 0)
            return "now";
        if (hoursUntilMatch == 1)
            return "in 1 hour";
        if (hoursUntilMatch <= 24)
            return $"in {hoursUntilMatch} hours";
        return "soon";
    }

    private static string SelectTemplate(MatchDetailDto match)
    {
        // Derby/Rivalry - highest priority
        if (match.RivalryScore >= 0.7)
            return GetRandomTemplate(DerbyTemplates);

        // Very high excitement
        if (match.ExcitmentScore >= 0.9)
            return GetRandomTemplate(MustWatchTemplates);

        // Title holder playing with good standings
        if (match.TitleHolderScore > 0 && match.CompetitionStandingScore >= 0.6)
            return GetRandomTemplate(TitleClashTemplates);

        // Top of table clash
        if (match.CompetitionStandingScore >= 0.8)
            return GetRandomTemplate(TopTableTemplates);

        // Both teams in good form
        if (match.FormScore >= 0.7)
            return GetRandomTemplate(FormTemplates);

        // High excitement
        if (match.ExcitmentScore >= 0.8)
            return GetRandomTemplate(HighQualityTemplates);

        // Default
        return GetRandomTemplate(GenericTemplates);
    }

    private static string GetRandomTemplate(string[] templates)
    {
        return templates[_random.Next(templates.Length)];
    }

    private static string FormatTemplate(string template, MatchDetailDto match, string timeText)
    {
        return template
            .Replace("{Home}", match.HomeTeamName)
            .Replace("{Away}", match.AwayTeamName)
            .Replace("{Time}", timeText);
    }

    private static string GenerateHashtag(MatchDetailDto match)
    {
        if (string.IsNullOrEmpty(match.CompetitionName))
            return string.Empty;

        var normalized = match.CompetitionName
            .Replace(" ", "")
            .Replace("-", "")
            .Replace(".", "");

        return $"#{normalized}";
    }

    // Templates - Natural language, conversational tone
    private static readonly string[] DerbyTemplates =
    [
        "Derby day is here. {Home} vs {Away} kicks off {Time}. You know what's at stake.",
        "The tension is building. {Home} faces {Away} {Time} in what promises to be a fierce battle.",
        "Rivalry renewed. {Home} vs {Away} {Time}. These two never disappoint.",
        "Local pride on the line. {Home} takes on {Away} {Time}. This one always delivers."
    ];

    private static readonly string[] MustWatchTemplates =
    [
        "This is going to be special. {Home} vs {Away} {Time}. Don't miss it.",
        "Clear your schedule. {Home} takes on {Away} {Time} and it has all the makings of a classic.",
        "Big one coming up. {Home} vs {Away} {Time}. This should be unmissable.",
        "Football at its best. {Home} vs {Away} {Time}. Everything points to a great match."
    ];

    private static readonly string[] TitleClashTemplates =
    [
        "Champions in action. {Home} vs {Away} {Time}. The title holders are playing.",
        "Title race implications. {Home} takes on {Away} {Time}. A crucial clash.",
        "The champions step up. {Home} vs {Away} kicks off {Time}.",
        "Defending champions face a test. {Home} vs {Away} {Time}."
    ];

    private static readonly string[] TopTableTemplates =
    [
        "Top of the table clash. {Home} vs {Away} {Time}. This one could shape the title race.",
        "Two high-flyers meet. {Home} vs {Away} {Time}. This one matters.",
        "The league leaders are involved. {Home} vs {Away} kicks off {Time}.",
        "A clash at the summit. {Home} takes on {Away} {Time}."
    ];

    private static readonly string[] FormTemplates =
    [
        "Both teams are flying right now. {Home} vs {Away} kicks off {Time}.",
        "Form says this will be entertaining. {Home} vs {Away} coming up {Time}.",
        "Two in-form sides collide. {Home} vs {Away} {Time}.",
        "Momentum on both sides. {Home} takes on {Away} {Time}."
    ];

    private static readonly string[] HighQualityTemplates =
    [
        "Worth watching today. {Home} vs {Away} kicks off {Time}.",
        "Quality football coming up. {Home} vs {Away} {Time}.",
        "This one looks good. {Home} vs {Away} {Time}.",
        "A match to keep an eye on. {Home} vs {Away} {Time}."
    ];

    private static readonly string[] GenericTemplates =
    [
        "Coming up: {Home} vs {Away} kicks off {Time}.",
        "{Home} vs {Away} {Time}. Could be a good one.",
        "On the schedule: {Home} takes on {Away} {Time}.",
        "Football today: {Home} vs {Away} {Time}."
    ];
}
