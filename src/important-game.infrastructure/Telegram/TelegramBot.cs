using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Telegram;

/*
 Limits:
    1,200 request weight per minute (keep in mind that this is not necessarily the same as 1,200 requests)
    100 orders per 10 seconds
    200,000 orders per 24 hours
 */
[ExcludeFromCodeCoverage]
public class TelegramBot : ITelegramBot
{
    private readonly HttpClient _client;
    private readonly TelegramOptions _options;
    private readonly ILogger<TelegramBot> _logger;

    public TelegramBot(HttpClient client, IOptions<TelegramOptions> options, ILogger<TelegramBot> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.BotToken))
        {
            _logger.LogDebug("Telegram bot token not configured; skipping message.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ChatId))
        {
            _logger.LogWarning("Telegram chat id not configured; skipping message.");
            return;
        }

        try
        {
            var chatId = Uri.EscapeDataString(_options.ChatId);
            var encodedMessage = Uri.EscapeDataString(message);
            var path = $"sendMessage?chat_id={chatId}&text={encodedMessage}&parse_mode=HTML";

            if (_client.BaseAddress is null)
            {
                var absoluteUri = new Uri($"https://api.telegram.org/bot{_options.BotToken}/{path}");
                using var absoluteResponse = await _client.GetAsync(absoluteUri).ConfigureAwait(false);
                await LogOnFailureAsync(absoluteResponse).ConfigureAwait(false);
                return;
            }

            using var response = await _client.GetAsync(path).ConfigureAwait(false);
            await LogOnFailureAsync(response).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending telegram message");
        }
    }

    private async Task LogOnFailureAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        _logger.LogError("Telegram API returned {StatusCode}: {Content}", response.StatusCode, content);
    }
}
