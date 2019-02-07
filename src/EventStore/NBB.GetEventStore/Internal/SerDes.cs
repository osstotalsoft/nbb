using System;
using Newtonsoft.Json;

namespace NBB.GetEventStore.Internal
{
    public class SerDes : ISerDes
    {
        public T Deserialize<T>(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public object Deserialize(byte[] data, Type type)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var obj = JsonConvert.DeserializeObject(json, type);
            return obj;
        }

        public byte[] Serialize(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}
