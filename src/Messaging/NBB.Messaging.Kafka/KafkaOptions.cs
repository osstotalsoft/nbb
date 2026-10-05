// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;

namespace NBB.Messaging.Kafka
{
    public class KafkaOptions
    {
        [ConfigurationKeyName("bootstrap_servers")]
        /// <summary>
        /// Comma-separated list of "host:port" pairs of one or more Kafka servers
        /// </summary>
        public string BootstrapServers { get; set; }

        [ConfigurationKeyName("group_id")]
        /// <summary>
        /// Identifier of the Kafka consumer group used by subscriptions
        /// </summary>
        public string GroupId { get; set; }
    }
}
