using System;

namespace NBB.ProcessManager.Definition.Effects
{
    public class CancelTimeouts : IEffect
    {
        public object InstanceId { get; }

        public CancelTimeouts(object instanceId)
        {
            InstanceId = instanceId;
        }
    }
}