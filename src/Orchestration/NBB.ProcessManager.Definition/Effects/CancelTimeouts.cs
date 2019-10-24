using System;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class CancelTimeouts : Effect
    {
        public object InstanceId { get; }
        public CancelTimeouts(object instanceId)
        {
            InstanceId = instanceId;
        }
    }
}