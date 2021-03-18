using System;
using Newtonsoft.Json;
using Quartz;

namespace AuctionInfoBot
{
    public class JobDataMapConverter
    {
        public JobDataMap Serialize<T>(T obj) where T : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var key = GetKey<T>();
            var json = JsonConvert.SerializeObject(obj);

            return new JobDataMap()
            {
                {key, json}
            };
        }

        public T Deserialize<T>(JobDataMap data) where T : class
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var key = GetKey<T>();
            var json = data.GetString(key);
            if (json == null) throw new InvalidOperationException($"No data with key {key} in JobDataMap");
            
            return JsonConvert.DeserializeObject<T>(json);
        }

        private string GetKey<T>()
        {
            return typeof(T).FullName;
        }
    }
}