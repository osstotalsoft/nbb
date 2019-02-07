using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NBB.Application.DataContracts
{
    public class ApplicationMetadata : Dictionary<string, string>
    {
        public const string CreationDateKey = "CreationDate";
        public const string CorrelationIdKey = "CorrelationId";

        protected virtual IEnumerable<string> GetPrivateKeys()
        {
            yield return CreationDateKey;
        }

        public IEnumerable<KeyValuePair<string, string>> GetPublicValues()
        {
            var privateKeys = GetPrivateKeys();
            return this.AsEnumerable().Where(x => !privateKeys.Contains(x.Key));
        }

        public DateTime CreationDate
        {
            get => TryGetValue<DateTime>(CreationDateKey);
            set => SetValue(CreationDateKey, value);
        }


        protected T TryGetValue<T>(string key)
        {
            if (TryGetValue(key, out var value))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    return (T)converter.ConvertFromInvariantString(value);
                }
            }

            return default(T);
        }

        protected void SetValue(String key, object value)
        {
            var converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter != null)
            {
                var stringValue = converter.ConvertToInvariantString(value);
                this[key] = stringValue;
            }
        }
    }
}