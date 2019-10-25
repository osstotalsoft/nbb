using System;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class CancelTimeoutsEffect : IEffect
    {
        public object InstanceId { get; }
        public CancelTimeoutsEffect(object instanceId)
        {
            InstanceId = instanceId;
        }
    }
}