using System;

namespace NBB.Messaging.Abstractions
{
    public record MessagingPublisherOptions
    {
        public Action<MessagingEnvelope> EnvelopeCustomizer { get; init; }

        public string TopicName { get; init; }

        public static MessagingPublisherOptions Default = new ();
   }
}
