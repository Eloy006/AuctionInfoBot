using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;

namespace NoSqlTorgiGovRu
{
    public class NotificationLotModel:DbLite<fullNotificationNotificationLot>
    {
        public const string CollectionName = "NotificationLots";
        public override string DataSetName => CollectionName;

        public IEnumerable<fullNotificationNotificationLot> FindByСadastralNubmer(string[] cadastrNum)
        {
            var col = _dbLite.GetCollection<fullNotificationNotificationLot>(DataSetName);

            if (cadastrNum.Length == 1)
            {
                var cadastr = cadastrNum.FirstOrDefault();
                var findOne = col.Include(x=>x.fullNotification).FindOne(x => x.cadastralNum == cadastr);

                if(findOne==null)return new fullNotificationNotificationLot[0];

                return new List<fullNotificationNotificationLot>() { findOne };
            }

            return col.Include(x=>x.fullNotification).Find(x => cadastrNum.Contains(x.cadastralNum)).ToArray();


        }


        private void UpdateLot(ILiteCollection<fullNotificationNotificationLot> col, fullNotificationNotificationLot oldLot,fullNotificationNotificationLot updateLot)
        {
            col.Update(new BsonValue(oldLot.fullNotificationNotificationLotId), updateLot);
        }

        public override bool Update(IEnumerable<fullNotificationNotificationLot> model)
        {
            var col = _dbLite.GetCollection<fullNotificationNotificationLot>(DataSetName);
            CreateIndex(col);
            col.Upsert(model);
            return true;
        }

        public override bool Update(IEnumerable<fullNotificationNotificationLot> model, bool forceUpdate)
        {
            throw new NotImplementedException();
        }

        protected override bool CreateIndex(ILiteCollection<fullNotificationNotificationLot> collection)
        {
            var created= collection.EnsureIndex(x => x.fullNotificationNotificationLotId);
            created&=collection.EnsureIndex(x => x.cadastralNum);
            return created;
        }
    }
}
