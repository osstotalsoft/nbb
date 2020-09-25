using System;
using NBB.Core.Effects;

namespace NBB.ProcessManager.Definition.SideEffects
{
    public static class Timeout
    {
        public static IEffect<Unit> Request<TMessage>(string instanceId, TimeSpan timeSpan, TMessage message)
            => Effect.Of(new RequestTimeout<TMessage>(instanceId, timeSpan, message));

        public static IEffect<Unit> Cancel(string instanceId)
            => Effect.Of(new CancelTimeouts(instanceId));
    }
}
