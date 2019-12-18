using System;
using NBB.Core.Effects;

namespace NBB.ProcessManager.Definition.SideEffects
{
    public static class Timeout
    {
        public static IEffect Request<TMessage>(string instanceId, TimeSpan timeSpan, TMessage message)
            => Effect.Of(new RequestTimeout<TMessage>(instanceId, timeSpan, message)).ToUnit();

        public static IEffect Cancel(string instanceId)
            => Effect.Of(new CancelTimeouts(instanceId)).ToUnit();
    }
}
