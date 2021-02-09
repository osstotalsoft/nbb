using System;
using System.Collections.Generic;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Host
{
    public class MessagingHostConfiguration
    {
        public List<SubscriberGroup> SubscriberGroups { get; } = new();

        public class SubscriberGroup
        {
            public List<(Type subscriberType, MessagingSubscriberOptions options)> Subscribers { get; } = new();
            public PipelineDelegate<MessagingContext> Pipeline { get; set; }
        }
    }
}