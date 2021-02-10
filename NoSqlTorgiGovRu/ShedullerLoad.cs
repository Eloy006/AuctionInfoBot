using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using NoSqlTorgiGovRu;
using WebXmlLoader;

namespace TorgiConsole
{
    public class ShedullerLoad
    {
        private openDataNotification LoadOdData(openDataNotification notification)
        {
            XmlLoader xmlLoader = new XmlLoader();

            try
            {
                var xmlData =  xmlLoader.LoadFromUrl<fullNotification>(notification.odDetailedHref);

                if (xmlData != null)
                {
                    notification.NotificationLoaded = true;
                    notification.FullNotification = xmlData;
                }

                return notification;


            }
            catch (Exception e)
            {
                notification.NotificationLoaded = false;
               
            }
            return null;
        }

        
        public int PartialLoad(int count)
        {
            IEnumerable<openDataNotification> openDataToLoad;
      
            var updateComplete = false;

            ConcurrentBag<openDataNotification>  paralelLoaded = new ConcurrentBag<openDataNotification>();

            using (var openData = new OpenDataNotificationModel())
            {
                openDataToLoad = openData.TakeDataToLoad(count);



                Parallel.ForEach(openDataToLoad, x =>
                    {
                        var loadedData = LoadOdData(x);
                        if(loadedData!=null)
                        paralelLoaded.Add(loadedData);
                    }
                );

                var loadedNotifications =
                    paralelLoaded.Where(x => x.NotificationLoaded).Select(x => x.FullNotification);


                using (var notificationModel=new FullNotificationModel())
                {
                    updateComplete = notificationModel.Update(loadedNotifications);
                }
                if(updateComplete) openData.Update(paralelLoaded, false);

                return loadedNotifications.Count();

            }
            

        }
        



    }
}
