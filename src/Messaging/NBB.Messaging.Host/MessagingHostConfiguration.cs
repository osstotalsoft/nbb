// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Host
{
    public class MessagingHostConfiguration
    {
        public List<Subscriber> Subscribers { get; } = new();

        public class Subscriber
        {
            public Type MessageType { get; set; }
            public MessagingSubscriberOptions Options { get; set; }
            public PipelineDelegate<MessagingContext> Pipeline { get; set; }
        }
    }
}
