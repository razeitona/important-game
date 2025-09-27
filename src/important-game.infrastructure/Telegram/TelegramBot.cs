using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace important_game.infrastructure.Telegram
{
    /*
     Limits:
        1,200 request weight per minute (keep in mind that this is not necessarily the same as 1,200 requests)
        100 orders per 10 seconds
        200,000 orders per 24 hours
     */
    public class TelegramBot
    {
        // You'll get these from setting up your bot
        private readonly HttpClient _client;
        private const string _botToken = "8148610785:AAEyE6D2xynWbnSYMlTsDjzDteuU5vCtInU";
        private const string _chatId = "-1002296682304";

        public TelegramBot()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri($"https://api.telegram.org/bot{_botToken}/")
            };
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {

                var parameters = new
                {
                    chat_id = _chatId,
                    text = message,
                    parse_mode = "HTML" // Allows basic HTML formatting
                };

                var url = $"sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(message)}";
                var response = await _client.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

    }
}