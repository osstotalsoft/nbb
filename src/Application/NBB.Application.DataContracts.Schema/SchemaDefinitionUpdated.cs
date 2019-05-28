using System.Collections.Generic;

namespace NBB.Application.DataContracts.Schema
{
    public class SchemaDefinitionUpdated : Event
    {
        public List<SchemaDefinition> Definitions { get; set; }
        public string ApplicationName { get; set; }

        public SchemaDefinitionUpdated(List<SchemaDefinition> definitions, string applicationName, EventMetadata metadata = null)
            : base(metadata)
        {
            Definitions = definitions;
            ApplicationName = applicationName;
        }
    }
}
