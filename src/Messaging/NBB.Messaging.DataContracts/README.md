# NBB.Messaging.DataContracts

This package helps us formalize and instrument messaging data contracts.

## NuGet install
```
dotnet add package NBB.Messaging.DataContracts
```

## Topic name attribute
Use this attribute to speciffy explicit topic name mapping for messages.

```csharp
[TopicName("my-custom-topic-name")]
public record ContractCreated(Guid ContractId, decimal Amount);
```