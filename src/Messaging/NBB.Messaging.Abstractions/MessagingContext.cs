namespace NBB.Messaging.Abstractions
{
    public class MessagingContext
    {
        public MessagingContext(MessagingEnvelope receivedMessageEnvelope)
        {
            ReceivedMessageEnvelope = receivedMessageEnvelope;
        }
        public MessagingEnvelope ReceivedMessageEnvelope { get; internal set; }
    }
}
