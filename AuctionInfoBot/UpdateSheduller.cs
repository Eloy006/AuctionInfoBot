using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NoSqlTorgiGovRu;
using TorgiConsole;
using WebXmlLoader;

namespace AuctionInfoBot
{
    public class UpdateSheduller
    {
        public async Task UpdateTask()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Begin load data");

                    var xmlLoader = new XmlLoader();

                    var dataX = await xmlLoader.LoadFromUrlAsync<openData>(
                        "https://torgi.gov.ru/opendata/7710349494-torgi/data.xml?bidKind=2&publishDateFrom=20201101T0000");
                    var total = 0;
                    using (var openData = new OpenDataNotificationModel())
                    {
                        openData.Update(dataX.notification);
                        total = openData.GetNotificationCount(false);
                    }

                    var shedullerLoad = new ShedullerLoad();


                    var current = 0;

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (true)
                    {
                        var loadPartial = shedullerLoad.PartialLoad(100);
                        current += loadPartial;
                        Console.Write($"\r{current} to {total}");
                        if (loadPartial == 0) break;
                        await Task.Delay(1000);
                    }

                    stopwatch.Stop();

                    Console.WriteLine("End load data");
                    await Task.Delay(1000 * 20 * 60);
                }
                catch (Exception _ex)
                {
                    await Task.Delay(10000);
                }
            }
        }
    }
}
