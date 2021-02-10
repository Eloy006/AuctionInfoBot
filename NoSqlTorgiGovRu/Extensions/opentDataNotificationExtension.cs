using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using WebXmlLoader;

namespace NoSqlTorgiGovRu
{
    partial class openDataNotification
    {
        [BsonId] public ObjectId openDataNotificationId { get; set; }

        public openDataNotification()
        {
            openDataNotificationId=ObjectId.NewObjectId();
        }
        
        public bool NotificationLoaded { get; set; }

        [BsonIgnore]
        public fullNotification FullNotification { get; set; }

    }
}
