using System;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class RequestTimeoutEffect : IEffect
    {
        public Type MessageType { get; }
        public object Message { get; }
        public string InstanceId { get; }
        public TimeSpan TimeSpan { get; }

        public RequestTimeoutEffect(string instanceId, TimeSpan timeSpan, object message, Type messageType)
        {
            InstanceId = instanceId;
            TimeSpan = timeSpan;
            Message = message;
            MessageType = messageType;
        }

        public Task<Unit> Accept(IEffectVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}