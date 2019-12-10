using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;
using NBB.ProcessManager.Definition.SideEffects;
using NBB.ProcessManager.Runtime.Timeouts;

namespace NBB.ProcessManager.Runtime.SideEffectHandlers
{
    public class CancelTimeoutsHandler : ISideEffectHandler<CancelTimeouts>
    {
        private readonly ITimeoutsRepository _timeoutsRepository;

        public CancelTimeoutsHandler(ITimeoutsRepository timeoutsRepository)
        {
            _timeoutsRepository = timeoutsRepository;
        }

        public Task Handle(CancelTimeouts sideEffect, CancellationToken cancellationToken = default)
        {
            return _timeoutsRepository.RemoveTimeoutBy(sideEffect.InstanceId);
        }
    }
}
