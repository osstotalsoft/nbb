// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Application.DataContracts.Schema.Sample;
using Newtonsoft.Json;
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
        public List<SchemaDefinition> GetSchemas(IEnumerable<Assembly> assemblies, Type baseType, Func<Type, string> topicResolver)
        {
            if (assemblies == null || !assemblies.Any())
            {
                throw new ArgumentException("assemblies", "Assemblies are null or empty");
            }

            if (topicResolver == null)
            {
                throw new ArgumentException("topicResolver", "TopicResolver must be specified");
            }

            if (baseType == null)
            {
                throw new ArgumentException("baseType", "BaseType must be specified");
            }

            var schemas = new List<SchemaDefinition>();
            var sampleBuilder = new SampleBuilder();

            foreach (var assembly in assemblies)
            {
                var assemblySchemas = assembly
                    .GetTypes().Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .Select(e =>
                    {
                        var schema = JsonSchema.FromType(e);
                        var jsonSchema = schema.ToJson();
                        var topic = topicResolver(e);

                        var obj = sampleBuilder.GetSampleFromType(e);
                        var sample = obj != null ? JsonConvert.SerializeObject(obj) : string.Empty;

                        return new SchemaDefinition(e.Name, e.FullName, jsonSchema, topic, sample);
                    })
                    .ToList();
                schemas.AddRange(assemblySchemas);
            }

            return schemas;
        }

        public SchemaDefinitionUpdated BuildSchemaUpdatedEvent(List<SchemaDefinition> schemas, string applicationName)
        {
            if (schemas == null || !schemas.Any())
            {
                throw new ArgumentException("schemas", "Schemas are null or empty");
            }

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("applicationName", "ApplicationName must be specified");
            }

            var @event = new SchemaDefinitionUpdated(schemas, applicationName);
            return @event;
        }

        public SchemaDefinition GetSchema(Type type, Func<Type, string> topicResolver)
        {
            if (topicResolver == null)
            {
                throw new ArgumentException("topicResolver", "TopicResolver must be specified");
            }

            if (type == null)
            {
                throw new ArgumentException("type", "Type must be specified");
            }

            var schema = JsonSchema.FromType(type);
            var jsonSchema = schema.ToJson();
            var topic = topicResolver(type);

            var sampleBuilder = new SampleBuilder();
            var obj = sampleBuilder.GetSampleFromType(type);
            var sample = obj != null ? JsonConvert.SerializeObject(obj) : string.Empty;

            return new SchemaDefinition(type.Name, type.FullName, jsonSchema, topic, sample);
        }
    }
}
