using LiteDB;

namespace NoSqlTorgiGovRu
{
    public partial class fullNotification
    {

        [BsonId] public string fullNotificationId => notification.bidNumber + "." + notification.bidOrganization.organizationId;
    }
}
