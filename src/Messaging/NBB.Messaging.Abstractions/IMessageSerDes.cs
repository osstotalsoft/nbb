namespace NBB.Messaging.Abstractions
{
    public interface IMessageSerDes
    {
        byte[] SerializeMessageEnvelope(MessagingEnvelope envelope, MessageSerDesOptions options = null);

        MessagingEnvelope<TMessage> DeserializeMessageEnvelope<TMessage>(byte[] envelopeData,
            MessageSerDesOptions options = null);

        MessagingEnvelope DeserializeMessageEnvelope(byte[] envelopeData, MessageSerDesOptions options = null) 
            => DeserializeMessageEnvelope<object>(envelopeData, options);
    }
}