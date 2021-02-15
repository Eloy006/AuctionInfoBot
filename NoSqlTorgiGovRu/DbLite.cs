using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using LiteDB;

namespace NoSqlTorgiGovRu
{
    public abstract class DbLite:IDisposable
    {
        private  string DEF_CONNECTION_STRING => @"Filename=Soyuz.db; Connection=shared";
        protected LiteDatabase _dbLite;

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
