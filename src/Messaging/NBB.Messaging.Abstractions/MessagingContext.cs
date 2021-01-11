using NBB.Messaging.DataContracts;
using System;

namespace NBB.Messaging.Abstractions
{
    public class MessagingContext
    {
        public Type PayloadType { get; private set; }
        public MessagingEnvelope ReceivedMessageEnvelope { get; internal set; }
        public string TopicName { get; internal set; }

        public MessagingContext(MessagingEnvelope receivedMessageEnvelope, Type payloadType, string topicName)
        {
            ReceivedMessageEnvelope = receivedMessageEnvelope;
            PayloadType = payloadType;
            TopicName = topicName;
        }
    }
}