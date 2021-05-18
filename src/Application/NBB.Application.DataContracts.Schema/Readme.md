# NBB.Application.DataContracts.Schema

This package provides json schema generator for data contracts. When exposing data contracts at runtime (HTTP or Messaging) you can use this package to generate json schema from CLR types 
## NuGet install
```
dotnet add package NBB.Application.DataContracts.Schema
```

## Example how to publish language at runtime, including schema, via messaging
```csharp
public class PublishLanguageHandler : IRequestHandler<PublishLanguage>
    {
        private readonly IMessageBusPublisher _messageBusPublisher;
        private readonly ITopicRegistry _topicRegistry;
        private readonly IConfiguration _configuration;
        private readonly ISchemaResolver _schemaResolver;

        public async Task<Unit> Handle(PublishLanguage request, CancellationToken cancellationToken)
        {
            var baseEventType = typeof(INotification);

            var schemaDefinitions =
                _schemaResolver.GetSchemas(new[] { typeof(MyCommand).Assembly }, baseEventType, type => _topicRegistry.GetTopicForMessageType(type, false));

            var schemaDefinitionUpdated = _schemaResolver.BuildSchemaUpdatedEvent(schemaDefinitions, GetSourceId());
            await _messageBusPublisher.PublishAsync(schemaDefinitionUpdated, cancellationToken);

            return Unit.Value;
        }

        private string GetSourceId()
        {
            var source = _configuration.GetSection("Messaging")?["Source"];
            return source ?? "";
        }
```

