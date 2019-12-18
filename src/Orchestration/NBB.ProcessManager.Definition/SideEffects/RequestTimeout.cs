using System;
using NBB.Core.Effects;

namespace NBB.ProcessManager.Definition.SideEffects
{
    public interface IRequestTimeoutHandler<TMessage> : ISideEffectHandler
    {
    }

    public class RequestTimeout<TMessage> : ISideEffect, IAmHandledBy<IRequestTimeoutHandler<TMessage>>
    {
        public string InstanceId { get; }
        public TimeSpan TimeSpan { get; }
        public TMessage Message { get; }

        public RequestTimeout(string instanceId, TimeSpan timeSpan, TMessage message)
        {
            InstanceId = instanceId;
            TimeSpan = timeSpan;
            Message = message;
        }
    }
}
