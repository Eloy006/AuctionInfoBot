using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using LiteDB;

namespace NoSqlTorgiGovRu
{
    public abstract class DbLite<T>:IDisposable
    {
        
        private  string DEF_CONNECTION_STRING => @"Filename=Soyuz.db; Connection=shared";
        protected LiteDatabase _dbLite;

        public abstract bool Update(IEnumerable<T> model);
        public abstract bool Update(IEnumerable<T> model,bool forceUpdate);
        public abstract string DataSetName { get; }
        protected abstract bool CreateIndex(ILiteCollection<T> collection);

        public DbLite()
        {
            _dbLite = new LiteDatabase(DEF_CONNECTION_STRING);
        }

 
        public void Dispose()
        {
            _dbLite?.Dispose();
        }
    }
}
