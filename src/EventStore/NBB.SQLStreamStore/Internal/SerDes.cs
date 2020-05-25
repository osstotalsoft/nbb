using System;
using Newtonsoft.Json;

namespace NBB.SQLStreamStore.Internal
{
    public class SerDes : ISerDes
    {
        public T Deserialize<T>(string data)
        {
            var obj = JsonConvert.DeserializeObject<T>(data);
            return obj;
        }

        public object Deserialize(string data, Type type)
        {
            var obj = JsonConvert.DeserializeObject(data, type);
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
