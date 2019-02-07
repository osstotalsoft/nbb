using System;
using System.Collections.Generic;

namespace NBB.Application.DataContracts.Schema
{
    public interface ISchemaResolver
    {
        List<SchemaDefinition> GetSchema(Type baseType, Func<Type, string> topicResolver = null);

        List<SchemaDefinition> GetSchema<T>(Func<Type, string> topicResolver = null);

        SchemaDefinitionUpdated GetSchemaAsEvent(Type baseType, Guid? correlationId, string applicationName,
           Func<Type, string> topicResolver = null);


        SchemaDefinitionUpdated GetSchemaAsEvent<T>(Guid? correlationId, string applicationName,
           Func<Type, string> topicResolver = null);
    }
}