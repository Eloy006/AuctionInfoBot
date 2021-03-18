using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace AuctionInfoBot
{
    public class TradeBotService : BackgroundService
    {
        private readonly TelegramBotClient _bot;
        private readonly TelegramBotUpdateHandler _botUpdateHandler;

        public TradeBotService(TelegramBotClient bot, TelegramBotUpdateHandler botUpdateHandler)
        {
            _bot = bot;
            _botUpdateHandler = botUpdateHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bot.ReceiveAsync(_botUpdateHandler, stoppingToken);
        }
    }
}