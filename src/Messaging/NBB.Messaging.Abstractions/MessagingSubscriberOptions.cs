using System.Collections.Generic;
using System.Reflection;

namespace NBB.Messaging.Abstractions
{
    public class MessagingSubscriberOptions
    {
        /// <summary>
        /// Options for how the messages are consumed.
        /// </summary>
        /// <value>
        /// The type of the consumer.
        /// </value>
        public MessagingConsumerType ConsumerType { get; set; } = MessagingConsumerType.CompetingConsumer;

        /// <summary>
        /// Opions for message acknowledgment.
        /// </summary>
        /// <value>
        /// The acknowledgment strategy.
        /// </value>
        public MessagingAcknowledgeStrategy AcknowledgeStrategy { get; set; } = MessagingAcknowledgeStrategy.Auto;

        /// <summary>
        /// Opions for message handling.
        /// </summary>
        /// <value>
        /// The handler strategy.
        /// </value>
        public MessagingHandlerStrategy HandlerStrategy { get; set; } = MessagingHandlerStrategy.Serial;

        /// <summary>
        /// Maximum number of messages received at time for a topic (default 1). 
        /// Increase the value when using the parallel HandlerStrategy
        /// </summary>
        /// <value>
        /// The handler strategy.
        /// </value>
        public int MaxInFlight { get; set; } = 1;

        /// <summary>
        /// Options for serialization/deserialization. See next section.
        /// </summary>
        /// <value>
        /// The serialization/deserialization options.
        /// </value>
        public MessageSerDesOptions SerDes { get; set; }
     }

    /// <summary>
    /// Used to configure the subscriber options
    /// </summary>
    public class MessagingSubscriberOptionsBuilder
    {
        /// <summary>
        /// Gets the subscriber options that are currently being configured.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public MessagingSubscriberOptions Options { get; }

        public MessagingSubscriberOptionsBuilder(MessagingSubscriberOptions options = null)
        {
            Options = new MessagingSubscriberOptions();

            if (options != null)
            {
                Options.ConsumerType = options.ConsumerType;
            }

            var serDesBuilder = new MessageSerDesOptionsBuilder(options?.SerDes);
            Options.SerDes = serDesBuilder.Options;
        }

    }

    /// <summary>
    /// Specify how the messages are consumed.
    /// </summary>
    public enum MessagingConsumerType
    {
        /// <summary>
        /// Messages are consumed by a single consumer in a consumer group
        /// </summary>
        CompetingConsumer = 0,
        /// <summary>
        /// Messages are consumed by all consumers in a consumer group
        /// </summary>
        CollaborativeConsumer = 1
    }

    /// <summary>
    /// Specify the strategy of message acknowledgment.
    /// </summary>
    public enum MessagingAcknowledgeStrategy
    {
        /// <summary>
        /// Acknowlegments are sent synchroniously after the message handler is called.
        /// </summary>
        Manual = 0,
        /// <summary>
        /// Messages are acknowledged automatically by the transport library (eg: Nats/Kafka auto commit).
        /// </summary>
        Auto = 1
    }

    /// <summary>
    /// Specify the strategy for message handling
    /// </summary>
    public enum MessagingHandlerStrategy
    {
        /// <summary>
        ///  The handler is called sinchroniously for each message. The next message is consumed after the current handler is finished.
        /// </summary>
        Serial = 0,
        /// <summary>
        ///  The handler is called asynchroniously for each message. It allows multiple messages to be handled in paralel.
        /// </summary>
        Parallel = 1
    }
}
