// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using NBB.Core.Pipeline;

namespace NBB.Messaging.Abstractions
{
    public record MessagingContext
        (MessagingEnvelope MessagingEnvelope, string TopicName, IServiceProvider Services) : IPipelineContext;
}