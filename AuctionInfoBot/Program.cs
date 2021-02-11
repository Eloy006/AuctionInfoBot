using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandTools;
using NoSqlTorgiGovRu;
using TorgiConsole;
using TorgiGovRu_Bot;
using TradeInformationBot;
using WebXmlLoader;


namespace AuctionInfoBot
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var xmlLoader = new XmlLoader();

            var dataX = await xmlLoader.LoadFromUrlAsync<openData>(
                "https://torgi.gov.ru/opendata/7710349494-torgi/data.xml?bidKind=2&publishDateFrom=20210101T0000");
            var total = 0;
            using (var openData = new OpenDataNotificationModel())
            { 
                openData.Update(dataX.notification);
                total = openData.GetNotificationCount(false);
            }

            var shedullerLoad = new ShedullerLoad();

            
            var current = 0;

            while (true)
            {
                var loadPartial = shedullerLoad.PartialLoad(100);
                current += loadPartial;
                Console.Write($"\r{current} to {total}");
                if (loadPartial == 0) break;
                Thread.Sleep(100);

            }

            TradeBot tradeBot=new TradeBot();
            tradeBot.Start();




            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
