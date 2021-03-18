using System;
using System.Threading;
using System.Threading.Tasks;
using AuctionInfoBot.Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AuctionInfoBot
{
    public class TelegramBotUpdateHandler : IUpdateHandler
    {
        private readonly JobDataMapConverter _jobDataMapConverter;
        private readonly IScheduler _scheduler;
        private readonly ILogger<TelegramBotUpdateHandler> _logger;

        public TelegramBotUpdateHandler(JobDataMapConverter jobDataMapConverter, IScheduler scheduler, ILogger<TelegramBotUpdateHandler> logger)
        {
            _jobDataMapConverter = jobDataMapConverter;
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var jobDataMap = _jobDataMapConverter.Serialize(update);
            
            var job = JobBuilder.Create<TelegramBotHandleUpdateJob>()
                .WithIdentity($"handle-update-job-{Guid.NewGuid():D}", "telegram-bot-group")
                .UsingJobData(jobDataMap)
                .Build();
            
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"handle-update-trigger-{Guid.NewGuid():D}", "telegram-bot-group")
                .StartNow()
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        public Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            switch (exception)
            {
                case ApiRequestException apiRequestException:
                    _logger.LogError(apiRequestException, $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}");
                    break;
                default:
                    _logger.LogError(exception, "Telegram.Bot Exception:");
                    break;
            }

            return Task.CompletedTask;
        }

        public UpdateType[] AllowedUpdates => null;
    }
}