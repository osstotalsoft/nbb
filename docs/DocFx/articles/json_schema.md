
Json schema
===============

Json schema is a standard that allows the definition of events and commands transfered between systems serialised as json.
https://json-schema.org/

Current implementation uses NJsonSchema as underlying implementation. (https://github.com/RicoSuter/NJsonSchema)

Advantages
----------------

Use of this package allows information exchange across systems written in different languages, such as C# and python. 

Example use case
----------------
A Python system can send its published language schema to a messaging system like Kafka or Nats. 
A C# system will listen to this topic and get this message.
The C# system can use reflection to generate typed C# code and send back information using typed structures or can parse the information and get the metadata.

Simple usage via JsonSchema package:
```csharp
var schema = await JsonSchema.FromTypeAsync<Person>();
var schemaData = schema.ToJson();
var errors = schema.Validate("{...}");

foreach (var error in errors)
    Console.WriteLine(error.Path + ": " + error.Kind);

schema = await JsonSchema.FromJsonAsync(schemaData);

```

NBB usage
----------------
The Nbb implementation thrives to offer a standard contract for communication via a typed event - NBB.Application.DataContracts.Schema.SchemaDefinitionUpdated.
This event contains a list of schema definitions and the application name / code / identifier for which the definition is updated.

```csharp
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
```

The NBB.Application.DataContracts.Schema.JsonSchemaResolver offers a convenient wrapper to get the definitions for a base type.

```csharp
public class UpdateCommandHandler : ICommandHandler<UpdatePublishedLanguage>{
	
	private readonlyISchemaResolver _resolver;
	private readonlyIMessageBusPublisher _messageBusPublisher;
	private readonly ITopicRegistry _topicRegistry;
	
	public UpdateCommandHandler(ISchemaResolver resolver, IMessageBusPublisher messageBusPublisher, ITopicRegistry topicRegistry)
	{
		_resolver = resolver;
		_messageBusPublisher = messageBusPublisher;
		_topicRegistry = topicRegistry;
	}
	
	public async Task Handle(UpdatePublishedLanguage command, CancellationToken token=default(CancellationToken))
	{
		var type = typeof(BaseEvent);
		var @event = _resolver.GetSchemaAsEvent(type, command.ApplicationName, (t) => topicRegistry.Resolve(t));
		await _messageBusPublisher.PublishAsync(@event);
		
		// OR
		// var list = _resolver.GetSchema(type, (t) => topicRegistry.Resolve(t));
		// var myEvent = new MyEvent(list, command.ApplicationName);
		// await _messageBusPublisher.PublishAsync(myEvent);
	}
}


```