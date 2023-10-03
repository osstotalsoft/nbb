// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Effects;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Effects
{
    public static class MessageBus
    {
        public static Effect<Unit> Publish(object message, MessagingPublisherOptions options = null) => Effect.Of<PublishMessage.SideEffect, Unit>(new PublishMessage.SideEffect(message, options));
    }
}
