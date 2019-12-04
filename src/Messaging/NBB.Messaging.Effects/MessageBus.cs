﻿using NBB.Core.Effects;

namespace NBB.Messaging.Effects
{
    public static class MessageBus
    {
        public static IEffect<Unit> Publish(object message) => Effect.Of(new PublishMessage.SideEffect(message));
    }
}
