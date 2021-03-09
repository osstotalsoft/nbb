using System;
using System.Collections.Generic;
using System.Reflection;

namespace NBB.Application.DataContracts.Schema
{
    public interface ISchemaResolver
    {
        List<SchemaDefinition> GetSchema(IEnumerable<Assembly> assemblies, Type baseType, Func<Type, string> topicResolver);
        SchemaDefinition GetSchema(Type type, Func<Type, string> topicResolver);

        SchemaDefinitionUpdated GetSchema(IEnumerable<Assembly> assemblies, Type baseType, string applicationName,
           Func<Type, string> topicResolver);
    }
}