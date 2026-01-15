using Microsoft.Extensions.Logging;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Utils
{
    /// <summary>
    /// Rate limiter for SofaScore API requests to prevent Cloudflare blocking.
    /// Ensures minimum 30 seconds between consecutive requests.
    /// </summary>
    public class SofaScoreRateLimiter
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan MinDelayBetweenRequests = TimeSpan.FromSeconds(30);

        private readonly ILogger<SofaScoreRateLimiter> _logger;

        public SofaScoreRateLimiter(ILogger<SofaScoreRateLimiter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes an API call with rate limiting to respect SofaScore's Cloudflare protection.
        /// Ensures minimum 30 seconds delay between consecutive requests.
        /// </summary>
        public async Task<T> ExecuteWithRateLimitAsync<T>(Func<Task<T>> apiCall, string operationName = "SofaScore API call")
        {
            await _semaphore.WaitAsync();

            try
            {
                var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;

                if (timeSinceLastRequest < MinDelayBetweenRequests)
                {
                    var delayNeeded = MinDelayBetweenRequests - timeSinceLastRequest;
                    _logger.LogDebug(
                        "Rate limiting {Operation}: waiting {Delay}ms before request",
                        operationName,
                        delayNeeded.TotalMilliseconds);

                    await Task.Delay(delayNeeded);
                }

                _logger.LogDebug("Executing {Operation}", operationName);
                var result = await apiCall();
                _lastRequestTime = DateTime.UtcNow;

                return result;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
