namespace NBB.Messaging.Kafka.Infrastructure
{

    public class KafkaMessagingOptions
    {
        public string MessageHandlerStrategyType { get; set; }
        public string MessageCommitStrategyType { get; set; }
    }
}
