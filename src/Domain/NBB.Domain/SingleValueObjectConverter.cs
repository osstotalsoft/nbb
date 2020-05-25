using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;

namespace NBB.Domain
{
    public class SingleValueObjectConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, Type> ConstructorArgumentTypes = new ConcurrentDictionary<Type, Type>();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identityValue = value?.GetType().GetProperty("Value")?.GetValue(value, null);

            serializer.Serialize(writer, identityValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var parameterType = ConstructorArgumentTypes[objectType];

            var value = serializer.Deserialize(reader, parameterType);
            return Activator.CreateInstance(objectType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                null, new[] {value}, CultureInfo.CurrentCulture);
        }

        public override bool CanConvert(Type objectType)
        {
            var currentType = objectType;
            while (currentType != null)
            {
                if (currentType.IsGenericType &&
                    currentType.GetGenericTypeDefinition() == typeof(SingleValueObject<>))
                {
                    ConstructorArgumentTypes[objectType] = currentType.GenericTypeArguments[0];
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }
    }

}
