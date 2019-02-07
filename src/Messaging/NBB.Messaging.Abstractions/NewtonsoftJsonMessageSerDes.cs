using System;
using NBB.Messaging.DataContracts;
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

        public MessagingEnvelope<TMessage> DeserializeMessageEnvelope<TMessage>(string envelopeString, MessageSerDesOptions options = null)
        {
            var serDesOptions = options ?? new MessageSerDesOptions();

            if (serDesOptions.DeserializationType == DeserializationType.HeadersOnly)
                throw new Exception("Cannot use typed deserialization using 'HeadersOnly' deserialization setting.");

            if (serDesOptions.DeserializationType == DeserializationType.Dynamic)
            {
                var partialEnvelope = DeserializePartialEnvelope(envelopeString, out var concreteType, serDesOptions,
                    typeof(TMessage));
                return new MessagingEnvelope<TMessage>(partialEnvelope.Headers,
                    (TMessage) partialEnvelope.Payload.ToObject(concreteType));
            }

            var message = JsonConvert.DeserializeObject<MessagingEnvelope<TMessage>>(envelopeString);
            return message;
        }

        public MessagingEnvelope DeserializeMessageEnvelope(string envelopeString, MessageSerDesOptions options = null)
        {
            var serDesOptions = options ?? new MessageSerDesOptions();

            if (serDesOptions.DeserializationType == DeserializationType.TypeSafe)
                throw new Exception("Cannot deserialize generic envelope using the TypeSafe deserialization option");

            var partialEnvelope = DeserializePartialEnvelope(envelopeString, out var concreteType, serDesOptions);

            return serDesOptions.DeserializationType == DeserializationType.HeadersOnly
                ? new MessagingEnvelope(partialEnvelope.Headers, partialEnvelope.Payload.ToString())
                : new MessagingEnvelope(partialEnvelope.Headers, partialEnvelope.Payload.ToObject(concreteType));
        }

        public string SerializeMessageEnvelope(MessagingEnvelope message, MessageSerDesOptions options = null)
        {
            var messageTypeId = _messageTypeRegistry.GetTypeId(message.Payload.GetType());
            message.SetHeader(MessagingHeaders.MessageType, messageTypeId, true);

            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        private MessagingEnvelope<JObject> DeserializePartialEnvelope(string envelopeString, out Type concreteType, MessageSerDesOptions serDesOptions, Type expectedType = null)
        {
            var partialEnvelope = JsonConvert.DeserializeObject<MessagingEnvelope<JObject>>(envelopeString);
            if (serDesOptions.DeserializationType == DeserializationType.HeadersOnly)
            {
                concreteType = null;
                return partialEnvelope;
            }

            var messageTypeId = partialEnvelope.GetMessageTypeId();
            if (messageTypeId == null)
            {
                if (expectedType == null || expectedType.IsAbstract || expectedType.IsInterface)
                    throw new Exception("Type information was not found in MessageType header");
                concreteType = expectedType;
                return partialEnvelope;
            }

            concreteType = _messageTypeRegistry.ResolveType(messageTypeId, serDesOptions.DynamicDeserializationScannedAssemblies);

            if (expectedType != null && !expectedType.IsAssignableFrom(concreteType))
                throw new Exception($"Incompatible types: expected {expectedType} and found {concreteType}");

            return partialEnvelope;
        }
    }
}
