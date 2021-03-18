using Telegram.Bot.Types;

namespace AuctionInfoBot.TradeBot.Messages
{
    public class EditedMessageRequest : RequestBase
    {
        public Message Message { get; }

        public EditedMessageRequest(Update update, Message message) : base(update)
        {
            Message = message;
        }
    }
}