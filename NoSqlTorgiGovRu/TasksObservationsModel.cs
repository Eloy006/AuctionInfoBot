using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlTorgiGovRu
{
    public class TasksObservationsModel:DbLite
    {
        private string DataSetName =>nameof(TasksObservationsModel);

        public int ChatId { get; set; }
        public int MessageId { get; set; }
        


    }
}
