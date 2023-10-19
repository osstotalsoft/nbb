// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Messaging.JetStream
{
    public class JetStreamOptions
    {
        /// <summary>
        /// URL of the Streaming NATS cluster
        /// </summary>
        public string NatsUrl { get; set; }

        /// <summary>
        /// Identifier of the Streaming NATS client
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The time the server awaits for acknowledgement from the client before redelivering the message (in milliseconds)
        /// </summary>
        public int? AckWait { get; set; }
        public string CommandsStream { get; set; }
        public string EventsStream { get; set; }

    }
}
