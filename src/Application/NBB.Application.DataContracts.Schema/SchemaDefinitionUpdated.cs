using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace NBB.Application.DataContracts.Schema
{
    public class SchemaDefinitionUpdated: IEvent
    {
        public List<SchemaDefinition> Definitions { get; set; }
        public string ApplicationName { get; set; }

        public Guid EventId { get;  }
        public Guid? CorrelationId { get;  }

        public ApplicationMetadata Metadata { get; set; } = new ApplicationMetadata();

        public SchemaDefinitionUpdated(List<SchemaDefinition> definitions, Guid? correlationId, Guid eventId, string applicationName )
        {
            Definitions = definitions;
            CorrelationId = correlationId;
            EventId = eventId;
            ApplicationName = applicationName;
        }
    }
}
