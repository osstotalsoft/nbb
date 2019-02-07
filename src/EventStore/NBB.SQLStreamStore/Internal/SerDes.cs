using System;
using Newtonsoft.Json;

namespace NBB.SQLStreamStore.Internal
{
    public class SerDes : ISerDes
    {
        public T Deserialize<T>(string json)
        {
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public object Deserialize(string json, Type type)
        {
            var obj = JsonConvert.DeserializeObject(json, type);
            return obj;
        }

        public string Serialize(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }
    }
}
