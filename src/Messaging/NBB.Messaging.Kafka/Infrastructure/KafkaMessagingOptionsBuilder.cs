namespace NBB.Messaging.Kafka.Infrastructure
{
    public class KafkaMessagingOptionsBuilder
    {
        public KafkaMessagingOptions Options { get; }

        public KafkaMessagingOptionsBuilder()
        {
            Options = new KafkaMessagingOptions();
        }

    }
}
