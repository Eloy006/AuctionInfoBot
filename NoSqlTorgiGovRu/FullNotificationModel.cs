using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;

namespace NoSqlTorgiGovRu
{
    public class FullNotificationModel:DbLite
    {
        private string DataSetName => "NotificationGround";
        public IEnumerable<fullNotification> FindByСadastralNubmer(string[] cadastrNum)
        {
            var col = _dbLite.GetCollection<fullNotification>(DataSetName);
            return col.Find(r => cadastrNum.Contains(r.notification.lot[0].cadastralNum));
        }
        
        public bool Update(IEnumerable<fullNotification> items)
        {
            var col = _dbLite.GetCollection<fullNotification>(DataSetName);
            CreateIndex(col);


            var finder = col.FindAll().ToArray();

            var inserts = GetDataToInsert(items, finder);

            var updates = GetDataToUpdate(items, finder);

            var cnUpdates = col.Update(updates);
            var cnInserts = col.InsertBulk(inserts);

            return cnUpdates == updates.Count && cnInserts == inserts.Count;
        }

        private static List<fullNotification> GetDataToUpdate(IEnumerable<fullNotification> items, fullNotification[] finder)
        {
            return (from item in items
                join findItem in finder on
                    item.fullNotificationId
                    equals
                    findItem.fullNotificationId
                select item).ToList();
        }

        private static List<fullNotification> GetDataToInsert(IEnumerable<fullNotification> items, fullNotification[] finder)
        {
            return items.Where(dataOpen => finder.All(x => x.fullNotificationId != dataOpen.fullNotificationId)).ToList();
        }

        private static void CreateIndex(ILiteCollection<fullNotification> col)
        {
            col.EnsureIndex(x => x.notification.bidNumber);
            col.EnsureIndex(x => x.notification.lot[0].cadastralNum);
        }
    }
}
