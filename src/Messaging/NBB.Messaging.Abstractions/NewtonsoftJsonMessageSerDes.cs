using NBB.Messaging.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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
                var partialEnvelope = DeserializePartialEnvelopeInternal(envelopeString);
                var concreteType = GetConcreteType(partialEnvelope, typeof(TMessage), serDesOptions);

                return new MessagingEnvelope<TMessage>(partialEnvelope.Headers,
                    (TMessage) partialEnvelope.Payload.ToObject(concreteType));
            }

            var message = JsonConvert.DeserializeObject<MessagingEnvelope<TMessage>>(envelopeString);
            return message;
        }

        public MessagingEnvelope DeserializePartialMessageEnvelope<TMessage>(string envelopeString)
        {         
            var partialEnvelope = DeserializePartialEnvelopeInternal(envelopeString);
            return partialEnvelope;
        }

        public MessagingEnvelope CompleteDeserialization(MessagingEnvelope partiallyDeserializedEnvelope, Type expectedType = null, MessageSerDesOptions options = null)
        {
            var serDesOptions = options ?? new MessageSerDesOptions();
            
            if (serDesOptions.DeserializationType == DeserializationType.TypeSafe && expectedType == null)
                throw new Exception("Cannot deserialize generic envelope using the TypeSafe deserialization option");

            if (serDesOptions.DeserializationType == DeserializationType.HeadersOnly && expectedType != null)
                throw new Exception("Cannot use typed deserialization using 'HeadersOnly' deserialization setting.");

            if (partiallyDeserializedEnvelope.Payload is JObject jObject) {
                var concreteType = GetConcreteType(partiallyDeserializedEnvelope, expectedType, serDesOptions);
                return serDesOptions.DeserializationType == DeserializationType.HeadersOnly
                    ? new MessagingEnvelope(partiallyDeserializedEnvelope.Headers, jObject.ToString())
                    : new MessagingEnvelope(partiallyDeserializedEnvelope.Headers, jObject.ToObject(concreteType));
            }

            throw new Exception("Partial deserialization payload is not in the expected format.");
        }

        public MessagingEnvelope DeserializeMessageEnvelope(string envelopeString, MessageSerDesOptions options = null)
        {
            var serDesOptions = options ?? new MessageSerDesOptions();

            if (serDesOptions.DeserializationType == DeserializationType.TypeSafe)
                throw new Exception("Cannot deserialize generic envelope using the TypeSafe deserialization option");
                        
            var partialEnvelope = DeserializePartialEnvelopeInternal(envelopeString);
            var concreteType = GetConcreteType(partialEnvelope, null, serDesOptions);

            return serDesOptions.DeserializationType == DeserializationType.HeadersOnly
                ? new MessagingEnvelope(partialEnvelope.Headers, partialEnvelope.Payload.ToString())
                : new MessagingEnvelope(partialEnvelope.Headers, partialEnvelope.Payload.ToObject(concreteType));
        }

        public MessagingEnvelope DeserializePartialMessageEnvelope(string envelopeString)
        {
            var partialEnvelope = DeserializePartialEnvelopeInternal(envelopeString);
            return partialEnvelope;
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

        private MessagingEnvelope<JObject> DeserializePartialEnvelopeInternal(string envelopeString)
        {
            var partialEnvelope = JsonConvert.DeserializeObject<MessagingEnvelope<JObject>>(envelopeString);
            return partialEnvelope;
        }

        private Type GetConcreteType(MessagingEnvelope partialEnvelope, Type expectedType, MessageSerDesOptions serDesOptions)
        {
            if (serDesOptions.DeserializationType == DeserializationType.HeadersOnly)
            {
                return null;
            }

            var messageTypeId = partialEnvelope.GetMessageTypeId();
            if (messageTypeId == null)
            {
                if (expectedType == null || expectedType.IsAbstract || expectedType.IsInterface)
                    throw new Exception("Type information was not found in MessageType header");
                return expectedType;
            }

            var concreteType = _messageTypeRegistry.ResolveType(messageTypeId, serDesOptions.DynamicDeserializationScannedAssemblies);

            if (expectedType != null && !expectedType.IsAssignableFrom(concreteType))
                throw new Exception($"Incompatible types: expected {expectedType} and found {concreteType}");

            return concreteType;
        }
    }
}
