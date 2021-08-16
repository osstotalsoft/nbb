// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Messaging.Abstractions
{
    public record SubscriptionTransportOptions
    {
        /// <summary>
        /// Specifies if a durable subscription should be made (default true).
        /// </summary>
        public bool IsDurable { get; init; } = true;

        /// <summary>
        /// Specifies if a consumer group should be used (default true).
        /// </summary>
        public bool UseGroup { get; init; } = true;

        /// <summary>
        /// Specifies the maximum number of messages that can be handled concurrently (default 1).
        /// </summary>
        public int MaxConcurrentMessages { get; init; } = 1;

        /// <summary>
        /// If set, only new messages are delivered. Otherwise all available messages are delivered even if published before the subscription.
        /// </summary>
        public bool DeliverNewMessagesOnly { get; set; } = true;

        /// <summary>
        /// Timeout before message redelivery (in milliseconds)
        /// </summary>
        public int? AckWait { get; init; } = null;

        /// <summary>
        /// Default transport options
        /// </summary>
        public static SubscriptionTransportOptions Default => new();

        /// <summary>
        /// Stream processor transport options (with durable subscription)
        /// </summary>
        public static SubscriptionTransportOptions StreamProcessor => new()
            {DeliverNewMessagesOnly = false};

        /// <summary>
        /// Request/Reply transport options (non-durable
        /// </summary>
        public static SubscriptionTransportOptions RequestReply => new()
            {IsDurable = false, UseGroup = false};

        public static SubscriptionTransportOptions PubSub => new()
            {IsDurable = false};
    }
}