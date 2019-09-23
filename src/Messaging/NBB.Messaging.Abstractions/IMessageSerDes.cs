using NBB.Messaging.DataContracts;
using System;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageSerDes
    {
        string SerializeMessageEnvelope(MessagingEnvelope envelope, MessageSerDesOptions options = null);
        MessagingEnvelope DeserializeMessageEnvelope(Type messageType, string envelopeString, MessageSerDesOptions options = null);
        MessagingEnvelope DeserializeMessageEnvelope(string envelopeString, MessageSerDesOptions options = null);
    }
}
