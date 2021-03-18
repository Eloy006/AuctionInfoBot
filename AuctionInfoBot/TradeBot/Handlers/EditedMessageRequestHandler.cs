using System;
using System.Threading;
using System.Threading.Tasks;
using AuctionInfoBot.TradeBot.Messages;
using MediatR;

namespace AuctionInfoBot.TradeBot.Handlers
{
    public class EditedMessageRequestHandler : AsyncRequestHandler<EditedMessageRequest>
    {
        protected override Task Handle(EditedMessageRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageRequestHandler : AsyncRequestHandler<MessageRequest>
    {
        protected override Task Handle(MessageRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}