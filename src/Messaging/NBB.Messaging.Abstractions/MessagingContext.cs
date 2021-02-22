using System;
using NBB.Core.Pipeline;

namespace NBB.Messaging.Abstractions
{
    public record MessagingContext
        (MessagingEnvelope MessagingEnvelope, string TopicName, IServiceProvider Services) : IPipelineContext;
}