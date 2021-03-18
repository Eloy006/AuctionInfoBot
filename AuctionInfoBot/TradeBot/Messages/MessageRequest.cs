using Telegram.Bot.Types;

namespace AuctionInfoBot.TradeBot.Messages
{
    public class MessageRequest : RequestBase
    {
        public Message Message { get; }

        public MessageRequest(Update update, Message message) : base(update)
        {
            Message = message;
        }
    }
}