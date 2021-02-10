using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandTools;
using NoSqlTorgiGovRu;
using TorgiConsole;
using TorgiGovRu_Bot;
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
                var loadPartial = shedullerLoad.PartialLoad(500);
                current += loadPartial;
                Console.Write($"\r{current} to {total}");
                if (loadPartial == 0) break;
                Thread.Sleep(5000);

            }

            

            
            CommandParser commandParser = new CommandParser();

            var parseData = commandParser.ParseCommand(BotCommandTask.CadastrCommands,
                "region \"камышин\" \"волгоград\" cadastr 10:15:0070102:177 69:24:0094601:350 20:17:0151001:411 publishdate 02.02.2020");
            using (var notification =new FullNotificationModel())
            {
                var fcadastr = notification.FindByСadastralNubmer(parseData[nameof(BotCommandTask.CadastrCommand.cadastr)]);
                var retDefault = fcadastr.ToArray();
            }
            
           

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
