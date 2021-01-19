﻿namespace NBB.Messaging.Abstractions
{
    public interface IMessageSerDes
    {
        string SerializeMessageEnvelope(MessagingEnvelope envelope, MessageSerDesOptions options = null);
        MessagingEnvelope<TMessage> DeserializeMessageEnvelope<TMessage>(string envelopeString, MessageSerDesOptions options = null);
        MessagingEnvelope DeserializeMessageEnvelope(string envelopeString, MessageSerDesOptions options = null);
    }
}
