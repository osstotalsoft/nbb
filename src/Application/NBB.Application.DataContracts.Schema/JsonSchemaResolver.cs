using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.Application.DataContracts.Schema
{
    /// <summary>
    /// Used to get the json schema (v4) of a given type
    /// </summary>
    public class JsonSchemaResolver : ISchemaResolver
    {
        public List<SchemaDefinition> GetSchema(Type baseType, Func<Type, string> topicResolver = null)
        {
            var list = baseType.Assembly
                .GetTypes().Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(e =>
                {
                    var schema = JsonSchema.FromType(e);
                    var jsonSchema = schema.ToJson();
                    var topic = topicResolver != null ? topicResolver(e) : string.Empty;

                    return new SchemaDefinition(e.Name, jsonSchema, topic);
                }).ToList();

            return list;
        }

        public List<SchemaDefinition> GetSchema<T>(Func<Type, string> topicResolver = null)
        {
            var baseType = typeof(T);
            return GetSchema(baseType, topicResolver);
        }

        public SchemaDefinitionUpdated GetSchemaAsEvent(Type baseType, string applicationName, Func<Type, string> topicResolver = null)
        {
            var list = GetSchema(baseType, topicResolver);
            var @event = new SchemaDefinitionUpdated(list, applicationName);
            return @event;
        }

        public SchemaDefinitionUpdated GetSchemaAsEvent<T>(string applicationName, Func<Type, string> topicResolver = null)
        {
            var baseType = typeof(T);
            var list = GetSchema(baseType, topicResolver);
            var @event = new SchemaDefinitionUpdated(list, applicationName);
            return @event;
        }
    }
}