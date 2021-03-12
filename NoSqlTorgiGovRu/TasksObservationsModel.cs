using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace NoSqlTorgiGovRu
{
    public class TasksObservationsModel:DbLite<TasksObservationsModel>
    {
        public override string DataSetName =>nameof(TasksObservationsModel);

        public int ChatId { get; set; }
        public int MessageId { get; set; }


        public override bool Update(IEnumerable<TasksObservationsModel> model)
        {
            throw new NotImplementedException();
        }

        public override bool Update(IEnumerable<TasksObservationsModel> model, bool forceUpdate)
        {
            throw new NotImplementedException();
        }

        protected override bool CreateIndex(ILiteCollection<TasksObservationsModel> collection)
        {
            throw new NotImplementedException();
        }
    }
}
