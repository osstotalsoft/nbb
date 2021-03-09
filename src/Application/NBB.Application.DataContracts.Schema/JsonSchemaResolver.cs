using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NBB.Application.DataContracts.Schema
{
    /// <summary>
    /// Used to get the json schema (v4) of a given type
    /// </summary>
    public class JsonSchemaResolver : ISchemaResolver
    {
        public List<SchemaDefinition> GetSchema(IEnumerable<Assembly> assemblies, Type baseType, Func<Type, string> topicResolver)
        {
            var schemas = new List<SchemaDefinition>();
            foreach (var assembly in assemblies)
            {
                var assemblySchemas = assembly
                    .GetTypes().Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .Select(e =>
                    {
                        var schema = JsonSchema.FromType(e);
                        var jsonSchema = schema.ToJson();
                        var topic = topicResolver(e);

                        return new SchemaDefinition(e.Name, e.FullName, jsonSchema, topic);
                    })
                    .ToList();
                schemas.AddRange(assemblySchemas);
            }

            return schemas;
        }

        public SchemaDefinitionUpdated GetSchema(IEnumerable<Assembly> assemblies, Type baseType, string applicationName, Func<Type, string> topicResolver)
        {
            var schemas = GetSchema(assemblies, baseType, topicResolver);
            var @event = new SchemaDefinitionUpdated(schemas, applicationName);
            return @event;
        }

        public SchemaDefinition GetSchema(Type type, Func<Type, string> topicResolver)
        {
            var schema = JsonSchema.FromType(type);
            var jsonSchema = schema.ToJson();
            var topic = topicResolver(type);

            return new SchemaDefinition(type.Name, type.FullName, jsonSchema, topic);
        }
    }
}