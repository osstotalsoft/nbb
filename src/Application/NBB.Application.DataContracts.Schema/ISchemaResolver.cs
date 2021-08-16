// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NBB.Application.DataContracts.Schema
{
    public interface ISchemaResolver
    {
        List<SchemaDefinition> GetSchemas(IEnumerable<Assembly> assemblies, Type baseType, Func<Type, string> topicResolver);
        SchemaDefinition GetSchema(Type type, Func<Type, string> topicResolver);

        SchemaDefinitionUpdated BuildSchemaUpdatedEvent(List<SchemaDefinition> schemas, string applicationName);
    }
}