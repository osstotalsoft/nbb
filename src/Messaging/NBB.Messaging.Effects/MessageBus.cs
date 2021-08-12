// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Effects;

namespace NBB.Messaging.Effects
{
    public static class MessageBus
    {
        public static Effect<Unit> Publish(object message) => Effect.Of<PublishMessage.SideEffect, Unit>(new PublishMessage.SideEffect(message));
    }
}
