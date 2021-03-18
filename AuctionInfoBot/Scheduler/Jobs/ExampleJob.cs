using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AuctionInfoBot.Scheduler.Jobs
{
    public class ExampleJob : IJob
    {
        private readonly ILogger<ExampleJob> _logger;

        public ExampleJob(ILogger<ExampleJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogCritical("LogCritical");
            _logger.LogError("LogError");
            _logger.LogWarning("LogWarning");
            _logger.LogInformation("LogInformation");
            _logger.LogDebug("LogDebug");
            _logger.LogTrace("LogTrace");

            throw new Exception("ololo");

            UpdateSheduller updateSheduller = new UpdateSheduller();

            Task.Factory.StartNew(() => updateSheduller.UpdateTask());
        }
    }
}