using System.Threading.Tasks;
using AuctionInfoBot.Scheduler;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using Telegram.Bot;

namespace AuctionInfoBot
{
    internal class Program
    {
        internal static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"))
                .ConfigureServices((context, services) =>
                {
                    services.Configure<Options>(context.Configuration.GetSection(Options.SectionKey));

                    services.AddQuartz(quartz =>
                    {
                        quartz.ConfigureQuartz();
                    });

                    services.AddQuartzHostedService(options =>
                    {
                        options.WaitForJobsToComplete = true;
                    });

                    services.AddMediatR(typeof(Program));

                    services.AddHostedService<TradeBotService>();
                    services.AddSingleton<AuctionBot>();
                    services.AddSingleton<TelegramBotClientFactory>();
                    services.AddSingleton<TelegramBotUpdateHandler>();
                    services.AddSingleton<TelegramBotClient>(provider => provider.GetRequiredService<TelegramBotClientFactory>().CreateTelegramBotClient());
                    services.AddSingleton<JobDataMapConverter>();
                })
                .UseConsoleLifetime()
                .Build()
                .RunAsync();
        }
    }
}
