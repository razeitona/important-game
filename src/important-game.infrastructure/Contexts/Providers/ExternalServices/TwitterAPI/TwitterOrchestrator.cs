using System.Diagnostics.CodeAnalysis;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;

[ExcludeFromCodeCoverage]
public class TwitterOrchestrator : ITwitterOrchestrator
{
    private readonly IMatchesRepository _matchesRepository;
    private readonly ITwitterIntegration _twitterIntegration;
    private readonly ITwitterPostGenerator _postGenerator;
    private readonly ITwitterPostRepository _twitterPostRepository;
    private readonly TwitterOptions _options;
    private readonly ILogger<TwitterOrchestrator> _logger;

    public TwitterOrchestrator(
        IMatchesRepository matchesRepository,
        ITwitterIntegration twitterIntegration,
        ITwitterPostGenerator postGenerator,
        ITwitterPostRepository twitterPostRepository,
        IOptions<TwitterOptions> options,
        ILogger<TwitterOrchestrator> logger)
    {
        _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
        _twitterIntegration = twitterIntegration ?? throw new ArgumentNullException(nameof(twitterIntegration));
        _postGenerator = postGenerator ?? throw new ArgumentNullException(nameof(postGenerator));
        _twitterPostRepository = twitterPostRepository ?? throw new ArgumentNullException(nameof(twitterPostRepository));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PostUpcomingExcitingMatchesAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Twitter integration is disabled");
            return;
        }

        try
        {
            _logger.LogInformation("Starting Twitter post orchestration for upcoming exciting matches");

            // Check monthly quota first
            var postsThisMonth = await _twitterPostRepository.GetPostCountForMonthAsync(DateTimeOffset.UtcNow);
            if (postsThisMonth >= _options.MaxPostsPerMonth)
            {
                _logger.LogWarning("Monthly tweet quota reached ({Count}/{Max}). No more posts will be made this month.",
                    postsThisMonth, _options.MaxPostsPerMonth);
                return;
            }

            var remainingQuota = _options.MaxPostsPerMonth - postsThisMonth;
            _logger.LogInformation("Monthly quota: {Used}/{Max} posts used, {Remaining} remaining",
                postsThisMonth, _options.MaxPostsPerMonth, remainingQuota);

            var unfinishedMatches = await _matchesRepository.GetAllUnfinishedMatchesAsync();

            if (unfinishedMatches == null || unfinishedMatches.Count == 0)
            {
                _logger.LogInformation("No upcoming matches found");
                return;
            }

            var eligibleMatches = FilterEligibleMatches(unfinishedMatches);

            _logger.LogInformation("Found {Count} eligible matches for Twitter posts", eligibleMatches.Count);

            var postedCount = 0;
            foreach (var match in eligibleMatches)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Check if we've reached quota during this run
                if (postedCount >= remainingQuota)
                {
                    _logger.LogInformation("Reached remaining monthly quota ({Remaining}). Stopping.", remainingQuota);
                    break;
                }

                // Check database for existing post (persistent duplicate check)
                var existingPost = await _twitterPostRepository.GetByMatchIdAsync(match.MatchId);
                if (existingPost != null)
                {
                    _logger.LogDebug("Match {MatchId} already posted (TweetId: {TweetId}), skipping",
                        match.MatchId, existingPost.TweetId);
                    continue;
                }

                var posted = await PostMatchAsync(match);
                if (posted)
                    postedCount++;
            }

            _logger.LogInformation("Twitter post orchestration completed. Posted {Count} tweets.", postedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Twitter post orchestration");
        }
    }

    private List<MatchDto> FilterEligibleMatches(List<MatchDto> matches)
    {
        var now = DateTimeOffset.UtcNow;
        var minTime = now;
        var maxTime = now.AddHours(_options.HoursBeforeMatchToPost);

        return matches
            .Where(m => m.ExcitmentScore >= _options.MinExcitementScoreToPost)
            .Where(m => m.MatchDateUTC >= minTime && m.MatchDateUTC <= maxTime)
            .OrderByDescending(m => m.ExcitmentScore)
            .ToList();
    }

    private async Task<bool> PostMatchAsync(MatchDto match)
    {
        try
        {
            var matchDetail = await _matchesRepository.GetMatchByIdAsync(match.MatchId);

            if (matchDetail == null)
            {
                _logger.LogWarning("Could not find match details for match {MatchId}", match.MatchId);
                return false;
            }

            var postContent = _postGenerator.GeneratePreMatchPost(matchDetail);
            var result = await _twitterIntegration.PostTweetAsync(postContent);

            if (result?.Data?.Id != null)
            {
                // Save to database for persistent duplicate tracking
                await _twitterPostRepository.SaveTwitterPostAsync(new TwitterPostEntity
                {
                    MatchId = match.MatchId,
                    TweetId = result.Data.Id,
                    Content = postContent,
                    PostedDateUTC = DateTimeOffset.UtcNow
                });

                _logger.LogInformation("Successfully posted tweet for match {MatchId}: {TweetId}",
                    match.MatchId, result.Data.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to post tweet for match {MatchId}", match.MatchId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting tweet for match {MatchId}", match.MatchId);
            return false;
        }
    }
}
