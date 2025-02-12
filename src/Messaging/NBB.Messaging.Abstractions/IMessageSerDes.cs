// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageSerDes
    {
        byte[] SerializeMessageEnvelope(MessagingEnvelope envelope, MessageSerDesOptions options = null);

        TMessage DeserializePayload<TMessage>(byte[] payloadBytes, IDictionary<string, string> medatada = null,
            MessageSerDesOptions options = null);

        (byte[] payloadBytes, IDictionary<string, string> additionalMetadata)
            SerializePayload<TMessage>(TMessage message, MessageSerDesOptions options = null);

        MessagingEnvelope<TMessage> DeserializeMessageEnvelope<TMessage>(byte[] envelopeData,
            MessageSerDesOptions options = null);

#if NETCOREAPP3_0_OR_GREATER
        MessagingEnvelope DeserializeMessageEnvelope(byte[] envelopeData, MessageSerDesOptions options = null)
            => DeserializeMessageEnvelope<object>(envelopeData, options);
#endif
    }

#if !NETCOREAPP3_0_OR_GREATER
    public static class MessageSerDesExtensions
    {

        public static MessagingEnvelope DeserializeMessageEnvelope(this IMessageSerDes serDes,
            byte[] envelopeData, MessageSerDesOptions options = null)
            => serDes.DeserializeMessageEnvelope<object>(envelopeData, options);
    }
#endif
}
