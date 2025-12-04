# NBB.Messaging.JetStream

NATS JetStream messaging transport implementation for NBB.

## Version History

**First introduced in NBB v7.0.12 (October 19, 2023)**

## Overview

JetStream was developed by NATS.io as the next-generation streaming platform with the following goals:

- **Easy to configure and operate** - The system must be observable
- **Secure** - Compatible with NATS 2.0 security models
- **Horizontally scalable** - Applicable to high ingestion rates
- **Multiple use cases** - Flexible for various scenarios
- **Self-healing** - Always available architecture
- **Native API** - Closer to core NATS
- **Stream integration** - NATS messages can be part of a stream
- **Payload agnostic** - No assumptions about message content
- **No third-party dependencies** - Pure NATS implementation

## Installation

```bash
dotnet add package NBB.Messaging.JetStream
```

## Configuration

Add JetStream as your messaging transport in your application's startup:

```csharp
services.AddMessagingTransport(JetStreamMessagingTransport.CreateTransport);
```

## Dependencies

This package uses the `NATS.Net` client library to communicate with NATS JetStream servers. See the project file for the specific version in use.

## Related Packages

- **NBB.Messaging.Abstractions** - Core messaging abstractions
- **NBB.Messaging.Host** - Messaging host infrastructure
- **NBB.Messaging.Nats** - Legacy NATS Streaming transport

## Migration from NATS Streaming

JetStream is the successor to NATS Streaming Server. If you are currently using `NBB.Messaging.Nats`, consider migrating to JetStream for improved features and long-term support.

## Resources

- [NATS JetStream Documentation](https://docs.nats.io/nats-concepts/jetstream)
- [NBB GitHub Repository](https://github.com/osstotalsoft/nbb)
