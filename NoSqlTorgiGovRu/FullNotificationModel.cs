using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace NoSqlTorgiGovRu
{
    public class FullNotificationModel : DbLite<fullNotification>
    {
        public const string CollectionName = "NotificationGround";
        public override string DataSetName => CollectionName;

        public fullNotificationNotificationLot[] CreateRefNotificationLot(fullNotification notification)
        {
            foreach (var lot in notification.notification.lot)
            {
                lot.fullNotification = notification;
            }

            return notification.notification.lot;
        }

        public override bool Update(IEnumerable<fullNotification> items)
        {
            var col = _dbLite.GetCollection<fullNotification>(DataSetName);
            
            CreateIndex(col);
            
            var finder = col.FindAll().ToArray();
            var inserts = GetDataToInsert(items, finder);
            var updates = GetDataToUpdate(items, finder);

            var updatesLot = updates.Where(x => x.notification?.lot != null)
                .SelectMany(CreateRefNotificationLot).ToList();
            updatesLot.AddRange(inserts.Where(x => x.notification?.lot != null)
                .SelectMany(CreateRefNotificationLot));
            
            var lotModel =new NotificationLotModel();
            lotModel.Update(updatesLot);
            
            var cnUpdates = col.Update(updates);
            var cnInserts = col.InsertBulk(inserts);
            return cnUpdates == updates.Count && cnInserts == inserts.Count;
        }

        public override bool Update(IEnumerable<fullNotification> model, bool forceUpdate)
        {
            return Update(model);
        }

        private static List<fullNotification> GetDataToUpdate(IEnumerable<fullNotification> items,
            fullNotification[] finder)
        {
            return (from item in items
                join findItem in finder on
                    item.fullNotificationId
                    equals
                    findItem.fullNotificationId
                select item).ToList();
        }

        private static List<fullNotification> GetDataToInsert(IEnumerable<fullNotification> items,
            fullNotification[] finder)
        {
            return items.Where(dataOpen => finder.All(x => x.fullNotificationId != dataOpen.fullNotificationId))
                .ToList();
        }

        protected override bool CreateIndex(ILiteCollection<fullNotification> col)
        {
            return col.EnsureIndex(x => x.notification.bidNumber);
        }
    }
}