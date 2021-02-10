using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;

namespace NoSqlTorgiGovRu
{
    public class OpenDataNotificationModel:DbLite
    {
        private string DataSetName => "openData";

        public openDataNotification ChangeId(openDataNotification oldOpenData, openDataNotification newOpenData, bool ReLoadNotification)
        {

            newOpenData.openDataNotificationId = oldOpenData.openDataNotificationId;
            if(ReLoadNotification)
                newOpenData.NotificationLoaded = false;
            return newOpenData;
        }

        public int GetNotificationCount(bool isLoad)
        {
            var col = _dbLite.GetCollection<openDataNotification>(DataSetName);
            return col.Count(x => x.NotificationLoaded == isLoad);
        }

        public IEnumerable<openDataNotification> TakeDataToLoad(int count)
        {
            var col = _dbLite.GetCollection<openDataNotification>(DataSetName);
            return col.Find(x => x.NotificationLoaded == false).Take(count).ToArray();
        }

        public bool Update(IEnumerable<openDataNotification> items,bool updateLastChange = true)
        {
            var col = _dbLite.GetCollection<openDataNotification>(DataSetName);

            CreateIndex(col);

            var finder = col.FindAll().ToArray();

            var inserts = GetDataToInsert(items, finder);
            var updates = GetDataToUpdate(items, finder, updateLastChange);

            var cnUpdates = col.Update(updates);
            var cnInserts = col.InsertBulk(inserts);

            return cnUpdates == updates.Count && cnInserts == inserts.Count;

        }

        private List<openDataNotification> GetDataToUpdate(IEnumerable<openDataNotification> items, openDataNotification[] finder,bool updateLastChange)
        {
            return (from item in items
                join findItem in finder on
                    new { item.publishDate, item.bidNumber, item.bidKindId }
                    equals
                    new { findItem.publishDate, findItem.bidNumber, findItem.bidKindId }
                where !updateLastChange || (findItem.lastChanged != item.lastChanged)
                select ChangeId(findItem, item, updateLastChange)).ToList();
        }

        private static List<openDataNotification> GetDataToInsert(IEnumerable<openDataNotification> items, openDataNotification[] finder)
        {
            return items.Where(dataOpen => !finder.Any(x => x.publishDate == dataOpen.publishDate
                                                            && x.bidNumber == dataOpen.bidNumber
                                                            && x.bidKindId == dataOpen.bidKindId)).ToList();
        }

        private static void CreateIndex(ILiteCollection<openDataNotification> col)
        {
            col.EnsureIndex("openDateIndex", "[$.bidKindId, $.bidNumber, $.bidKindId]");
            col.EnsureIndex(x => x.NotificationLoaded);
        }
    }
}
