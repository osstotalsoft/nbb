// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NBB.Messaging.Abstractions
{
    public class NewtonsoftJsonMessageSerDes : IMessageSerDes
    {
        private readonly IMessageTypeRegistry _messageTypeRegistry;

        public NewtonsoftJsonMessageSerDes(IMessageTypeRegistry messageTypeRegistry)
        {
            _messageTypeRegistry = messageTypeRegistry;
        }

        public TMessage DeserializePayload<TMessage>(byte[] payloadBytes, IDictionary<string, string> metadata = null,
            MessageSerDesOptions options = null)
        {
            options ??= MessageSerDesOptions.Default;
            var payload = Deserialize<JObject>(payloadBytes);
            var messageTypeId = new MessagingEnvelope(null, metadata).GetMessageTypeId();
            var outputType = ResolveOutputType<TMessage>(messageTypeId, typeof(TMessage), options);

            return (TMessage)payload.ToObject(outputType);
        }

        public (byte[] payloadBytes, IDictionary<string, string> additionalMetadata)
            SerializePayload<TMessage>(TMessage message, MessageSerDesOptions options = null)
        {
            var messageTypeId = _messageTypeRegistry.GetTypeId(message.GetType());
            var additionalMetadata = new Dictionary<string, string> { { MessagingHeaders.MessageType, messageTypeId } };

            return (Serialize(message), additionalMetadata);
        }

        public MessagingEnvelope<TMessage> DeserializeMessageEnvelope<TMessage>(byte[] envelopeData,
            MessageSerDesOptions options = null)
        {
            options ??= MessageSerDesOptions.Default;
            var partialEnvelope = Deserialize<MessagingEnvelope<JObject>>(envelopeData);
            var messageTypeId = partialEnvelope.GetMessageTypeId();
            var outputType = ResolveOutputType<TMessage>(messageTypeId, typeof(TMessage), options);

            return new MessagingEnvelope<TMessage>(partialEnvelope.Headers,
                (TMessage)partialEnvelope.Payload.ToObject(outputType));
        }

        public byte[] SerializeMessageEnvelope(MessagingEnvelope message, MessageSerDesOptions options = null)
        {
            var messageTypeId = _messageTypeRegistry.GetTypeId(message.Payload.GetType());
            message.SetHeader(MessagingHeaders.MessageType, messageTypeId, true);

            return Serialize(message);
        }
        private Type ResolveOutputType<TMessage>(string messageTypeId, Type expectedType, MessageSerDesOptions options = null)
        {
            var runtimeType = ResolveRuntimeType(messageTypeId, expectedType, options);

            return runtimeType ?? (typeof(TMessage) == typeof(object) ? typeof(ExpandoObject) : typeof(TMessage));
        }

        private Type ResolveRuntimeType(string messageTypeId, Type expectedType, MessageSerDesOptions options = null)
        {
            if (!options.UseDynamicDeserialization)
                return null;

            if (messageTypeId == null)
            {
                if (expectedType == null || expectedType.IsAbstract || expectedType.IsInterface)
                    throw new Exception("Type information was not found in MessageType header");

                return null;
            }

            var runtimeType =
                _messageTypeRegistry.ResolveType(messageTypeId, options.DynamicDeserializationScannedAssemblies);

            if (runtimeType == null)
                return null;

            if (expectedType != null && !expectedType.IsAssignableFrom(runtimeType))
                throw new Exception($"Incompatible types: expected {expectedType} and found {runtimeType}");

            return runtimeType;
        }

        private T Deserialize<T>(byte[] data)
        {
            var envelopeString = System.Text.Encoding.UTF8.GetString(data);

            return JsonConvert.DeserializeObject<T>(envelopeString);
        }

        private byte[] Serialize<T>(T message)
        {
            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}
