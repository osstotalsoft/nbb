using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NBB.EventStore.Internal
{
    public class NewtonsoftJsonEventStoreSerDes : IEventStoreSerDes
    {
        private readonly List<JsonConverter> _converters;

        public NewtonsoftJsonEventStoreSerDes(IEnumerable<JsonConverter> converters = null)
        {
            _converters = converters?.ToList() ?? new List<JsonConverter>();
        }

        public string Serialize(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = _converters
            });

            return json;
        }

        public object Deserialize(string strObj, Type type)
        {
            var obj = JsonConvert.DeserializeObject(strObj, type, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = _converters
            });
            return obj;
        }

    }


}
