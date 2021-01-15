using System.Collections.Generic;
using MediatR;

namespace NBB.Application.DataContracts.Schema
{
    public record SchemaDefinitionUpdated(
        List<SchemaDefinition> Definitions,
        string ApplicationName
    ) : INotification;
}
