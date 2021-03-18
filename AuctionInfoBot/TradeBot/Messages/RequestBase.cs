using MediatR;
using Telegram.Bot.Types;

namespace AuctionInfoBot.TradeBot.Messages
{
    public class RequestBase : IRequest
    {
        public Update Update { get; }

        public RequestBase(Update update)
        {
            Update = update;
        }
    }
}