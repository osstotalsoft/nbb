// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
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

        public MessagingEnvelope<TMessage> DeserializeMessageEnvelope<TMessage>(byte[] envelopeData, MessageSerDesOptions options = null)
        {
            var envelopeString = System.Text.Encoding.UTF8.GetString(envelopeData);
            var (partialEnvelope, runtimeType) = DeserializePartialEnvelope(envelopeString, typeof(TMessage), options);

            var outputType = runtimeType ??
                             (typeof(TMessage) == typeof(object) ? typeof(ExpandoObject) : typeof(TMessage));

            return new MessagingEnvelope<TMessage>(partialEnvelope.Headers,
                (TMessage) partialEnvelope.Payload.ToObject(outputType));
        }

        public byte[] SerializeMessageEnvelope(MessagingEnvelope message, MessageSerDesOptions options = null)
        {
            var messageTypeId = _messageTypeRegistry.GetTypeId(message.Payload.GetType());
            message.SetHeader(MessagingHeaders.MessageType, messageTypeId, true);

            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        private (MessagingEnvelope<JObject> envelope, Type runtimeType) DeserializePartialEnvelope(
            string envelopeString, Type expectedType = null, MessageSerDesOptions options = null)
        {
            options ??= MessageSerDesOptions.Default;
            var partialEnvelope = JsonConvert.DeserializeObject<MessagingEnvelope<JObject>>(envelopeString);

            if (!options.UseDynamicDeserialization)
                return (partialEnvelope, null);

            var messageTypeId = partialEnvelope.GetMessageTypeId();
            if (messageTypeId == null)
            {
                if (expectedType == null || expectedType.IsAbstract || expectedType.IsInterface)
                    throw new Exception("Type information was not found in MessageType header");

                return (partialEnvelope, null);
            }

            var runtimeType =
                _messageTypeRegistry.ResolveType(messageTypeId, options.DynamicDeserializationScannedAssemblies);

            if (runtimeType == null)
                return (partialEnvelope, null);

            if (expectedType != null && !expectedType.IsAssignableFrom(runtimeType))
                throw new Exception($"Incompatible types: expected {expectedType} and found {runtimeType}");

            return (partialEnvelope, runtimeType);
        }
    }
}