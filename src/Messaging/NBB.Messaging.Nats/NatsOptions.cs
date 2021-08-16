// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Messaging.Nats
{
    public class NatsOptions
    {
        /// <summary>
        /// URL of the Streaming NATS cluster
        /// </summary>
        public string NatsUrl { get; set; }

        /// <summary>
        /// Streaming NATS cluster name
        /// </summary>
        public string Cluster { get; set; }

        /// <summary>
        /// Identifier of the Streaming NATS client
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Durable subscriptions name.
        /// </summary>
        public string DurableName { get; set; }

        /// <summary>
        /// Queue group name
        /// </summary>
        public string QGroup { get; set; }

        /// <summary>
        /// The time the server awaits for acknowledgement from the client before redelivering the message (in milliseconds)
        /// </summary>
        public int? AckWait { get; set; }
    }
}