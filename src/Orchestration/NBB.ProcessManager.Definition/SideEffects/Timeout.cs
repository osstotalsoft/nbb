using System;
using NBB.Core.Effects;

namespace NBB.ProcessManager.Definition.SideEffects
{
    public static class Timeout
    {
        public static Effect<Unit> Request<TMessage>(string instanceId, TimeSpan timeSpan, TMessage message)
            => Effect.Of<RequestTimeout<TMessage>,Unit>(new RequestTimeout<TMessage>(instanceId, timeSpan, message));

        public static Effect<Unit> Cancel(string instanceId)
            => Effect.Of<CancelTimeouts, Unit>(new CancelTimeouts(instanceId));
    }
}
