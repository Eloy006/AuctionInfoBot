using System;
using System.Threading.Tasks;
using AuctionInfoBot.TradeBot.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AuctionInfoBot.Scheduler.Jobs
{
    public class TelegramBotHandleUpdateJob : IJob
    {
        private readonly JobDataMapConverter _jobDataMapConverter;
        private readonly IMediator _mediator;
        private readonly ILogger<TelegramBotHandleUpdateJob> _logger;

        public TelegramBotHandleUpdateJob(JobDataMapConverter jobDataMapConverter, IMediator mediator, ILogger<TelegramBotHandleUpdateJob> logger)
        {
            _jobDataMapConverter = jobDataMapConverter;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var update = _jobDataMapConverter.Deserialize<Update>(context.JobDetail.JobDataMap);

            IRequest request = update.Type switch
            {
                UpdateType.Message => new MessageRequest(update, update.Message),
                UpdateType.EditedMessage => new EditedMessageRequest(update, update.EditedMessage),
                // UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                // UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
                // UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                _ => throw new NotSupportedException($"Unknown update type: {update.Type}")
            };

            await _mediator.Send(request, context.CancellationToken);
        }
    }
}