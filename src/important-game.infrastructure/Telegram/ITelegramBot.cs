using System.Threading.Tasks;

namespace important_game.infrastructure.Telegram
{
    public interface ITelegramBot
    {
        Task SendMessageAsync(string message);
    }
}
