using NBB.Messaging.DataContracts;
using System;

namespace NBB.Messaging.Abstractions
{
    public record MessagingContext(
        MessagingEnvelope ReceivedMessageEnvelope,
        Type PayloadType,
        string TopicName,
        MessageSerDesOptions serDesOptions
        );
}