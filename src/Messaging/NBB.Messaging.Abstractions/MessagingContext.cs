namespace NBB.Messaging.Abstractions
{
    public class MessagingContext
    {
        public MessagingContext(MessagingEnvelope receivedMessageEnvelope, string topicName)
        {
            ReceivedMessageEnvelope = receivedMessageEnvelope;
            TopicName = topicName;
        }
        public MessagingEnvelope ReceivedMessageEnvelope { get; internal set; }
        public string TopicName { get; internal set; }
    }
}
