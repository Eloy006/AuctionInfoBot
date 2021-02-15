using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace NoSqlTorgiGovRu  
{
    partial class fullNotificationNotificationLot
    {
        [BsonId] public string fullNotificationNotificationLotId => cadastralNum + "." + id;
    }
}
