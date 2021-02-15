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
            UpdateSheduller updateSheduller=new UpdateSheduller();

            Task.Factory.StartNew(() => updateSheduller.UpdateTask());
            
            TradeBot tradeBot=new TradeBot();
            tradeBot.Start();




            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
