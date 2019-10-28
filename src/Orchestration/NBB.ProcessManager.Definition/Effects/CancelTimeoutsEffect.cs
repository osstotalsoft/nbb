using System;
using System.Threading.Tasks;
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

        public Task<Unit> Accept(IEffectVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}