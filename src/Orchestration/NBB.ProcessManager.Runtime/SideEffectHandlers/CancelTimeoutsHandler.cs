// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;
using NBB.ProcessManager.Definition.SideEffects;
using NBB.ProcessManager.Runtime.Timeouts;

namespace NBB.ProcessManager.Runtime.SideEffectHandlers
{
    public class CancelTimeoutsHandler : ISideEffectHandler<CancelTimeouts, Unit>
    {
        private readonly ITimeoutsRepository _timeoutsRepository;

        public CancelTimeoutsHandler(ITimeoutsRepository timeoutsRepository)
        {
            _timeoutsRepository = timeoutsRepository;
        }

        public async Task<Unit> Handle(CancelTimeouts sideEffect, CancellationToken cancellationToken = default)
        {
            await _timeoutsRepository.RemoveTimeoutBy(sideEffect.InstanceId);
            return Unit.Value;
        }
    }
}
