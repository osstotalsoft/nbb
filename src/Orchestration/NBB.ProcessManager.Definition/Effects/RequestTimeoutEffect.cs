using System;

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
    }
}