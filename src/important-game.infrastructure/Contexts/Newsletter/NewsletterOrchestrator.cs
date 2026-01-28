using System.Diagnostics.CodeAnalysis;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Newsletter.Data;
using important_game.infrastructure.Contexts.Newsletter.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace important_game.infrastructure.Contexts.Newsletter;

public interface INewsletterOrchestrator
{
    Task SendWeeklyNewsletterAsync(CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public class NewsletterOrchestrator : INewsletterOrchestrator
{
    private readonly INewsletterRepository _newsletterRepository;
    private readonly IMatchesRepository _matchesRepository;
    private readonly IEmailService _emailService;
    private readonly INewsletterTemplateGenerator _templateGenerator;
    private readonly EmailOptions _options;
    private readonly ILogger<NewsletterOrchestrator> _logger;

    private const int TopMatchesCount = 5;
    private const int UpcomingDaysRange = 7;

    public NewsletterOrchestrator(
        INewsletterRepository newsletterRepository,
        IMatchesRepository matchesRepository,
        IEmailService emailService,
        INewsletterTemplateGenerator templateGenerator,
        IOptions<EmailOptions> options,
        ILogger<NewsletterOrchestrator> logger)
    {
        _newsletterRepository = newsletterRepository ?? throw new ArgumentNullException(nameof(newsletterRepository));
        _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _templateGenerator = templateGenerator ?? throw new ArgumentNullException(nameof(templateGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendWeeklyNewsletterAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Newsletter sending is disabled");
            return;
        }

        try
        {
            _logger.LogInformation("Starting weekly newsletter send process");

            // Get active subscribers
            var subscribers = await _newsletterRepository.GetActiveSubscribersAsync(cancellationToken);

            if (subscribers.Count == 0)
            {
                _logger.LogInformation("No active subscribers found. Skipping newsletter send.");
                return;
            }

            _logger.LogInformation("Found {Count} active subscribers", subscribers.Count);

            // Get top matches for the upcoming week
            var topMatches = await GetTopMatchesForWeekAsync(cancellationToken);

            if (topMatches.Count == 0)
            {
                _logger.LogWarning("No upcoming matches found for newsletter. Skipping send.");
                return;
            }

            _logger.LogInformation("Found {Count} top matches for newsletter", topMatches.Count);

            // Prepare recipients
            var recipients = subscribers.Select(s => new EmailRecipient
            {
                Email = s.Email,
                UnsubscribeToken = s.UnsubscribeToken
            }).ToList();

            // Send emails
            var subject = $"This Week's Most Exciting Matches - {DateTime.UtcNow:MMMM d, yyyy}";

            var results = await _emailService.SendBulkEmailAsync(
                recipients,
                subject,
                recipient => _templateGenerator.GenerateWeeklyNewsletter(topMatches, recipient.UnsubscribeToken),
                cancellationToken);

            // Log results
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            _logger.LogInformation(
                "Newsletter send completed. Total: {Total}, Success: {Success}, Failed: {Failed}",
                results.Count, successCount, failureCount);

            // Log failures
            foreach (var failure in results.Where(r => !r.Success))
            {
                _logger.LogWarning("Failed to send newsletter to {Email}: {Error}",
                    failure.Email, failure.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during newsletter send process");
            throw;
        }
    }

    private async Task<List<MatchDetailDto>> GetTopMatchesForWeekAsync(CancellationToken cancellationToken)
    {
        // Get all unfinished matches
        var allMatches = await _matchesRepository.GetAllUnfinishedMatchesAsync();

        if (allMatches == null || allMatches.Count == 0)
        {
            return new List<MatchDetailDto>();
        }

        var now = DateTimeOffset.UtcNow;
        var weekEnd = now.AddDays(UpcomingDaysRange);

        // Filter matches within the next week and sort by excitement score
        var upcomingMatchIds = allMatches
            .Where(m => m.MatchDateUTC >= now && m.MatchDateUTC <= weekEnd)
            .OrderByDescending(m => m.ExcitmentScore)
            .Take(TopMatchesCount)
            .Select(m => m.MatchId)
            .ToList();

        if (upcomingMatchIds.Count == 0)
        {
            return new List<MatchDetailDto>();
        }

        // Get full match details for each match
        var matchDetails = new List<MatchDetailDto>();

        foreach (var matchId in upcomingMatchIds)
        {
            var matchDetail = await _matchesRepository.GetMatchByIdAsync(matchId);
            if (matchDetail != null)
            {
                matchDetails.Add(matchDetail);
            }
        }

        // Sort again to ensure order (in case of any data inconsistency)
        return matchDetails.OrderByDescending(m => m.ExcitmentScore).ToList();
    }
}
