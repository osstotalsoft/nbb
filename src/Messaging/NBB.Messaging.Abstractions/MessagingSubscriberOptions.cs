namespace NBB.Messaging.Abstractions
{
    public record MessagingSubscriberOptions
    {
        /// <summary>
        /// Options for serialization/deserialization. See next section.
        /// </summary>
        /// <value>
        /// The serialization/deserialization options.
        /// </value>
        public MessageSerDesOptions SerDes { get; init; }

        /// <summary>
        /// Options for the messaging transport
        /// </summary>
        public SubscriptionTransportOptions Transport { get; init; } = SubscriptionTransportOptions.Default;

        /// <summary>
        /// The name of the topic to subscribe to (optional for typed subscriptions)
        /// </summary>
        public string TopicName { get; init; }

        /// <summary>
        /// Default values for the messaging subscription options
        /// </summary>
        public static MessagingSubscriberOptions Default { get; } = new();
    }
}
