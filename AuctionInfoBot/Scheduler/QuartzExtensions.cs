using System;
using AuctionInfoBot.Scheduler.Jobs;
using Quartz;

namespace AuctionInfoBot.Scheduler
{
    public static class QuartzExtensions
    {
        public static void ConfigureQuartz(this IServiceCollectionQuartzConfigurator quartz)
        {
            quartz.SchedulerId = "Scheduler-Default";

            quartz.UseMicrosoftDependencyInjectionScopedJobFactory(options =>
            {
                options.AllowDefaultConstructor = false;
                options.CreateScope = true;
            });

            quartz.UseSimpleTypeLoader();
            quartz.UseInMemoryStore();
            quartz.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });

            quartz.ConfigureQuartzJobs();
        }

        private static void ConfigureQuartzJobs(this IServiceCollectionQuartzConfigurator quartz)
        {
            quartz.ScheduleJob<ExampleJob>(trigger =>
                trigger.WithSimpleSchedule(x =>
                        x.WithInterval(TimeSpan.FromSeconds(10))
                            .RepeatForever())
                    .StartNow()
            );
        }
    }
}