using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;
using NBB.ProcessManager.Definition.SideEffects;
using NBB.ProcessManager.Runtime.Timeouts;

namespace NBB.ProcessManager.Runtime.SideEffectHandlers
{
    public class RequestTimeoutHandler<TMessage> :  ISideEffectHandler<RequestTimeout<TMessage>>, IRequestTimeoutHandler<TMessage>
    {
        private readonly TimeoutsManager _timeoutsManager;
        private readonly ITimeoutsRepository _timeoutsRepository;
        private readonly Func<DateTime> _currentTimeProvider;


        public RequestTimeoutHandler(TimeoutsManager timeoutsManager, ITimeoutsRepository timeoutsRepository, Func<DateTime> currentTimeProvider)
        {
            _timeoutsManager = timeoutsManager;
            _timeoutsRepository = timeoutsRepository;
            _currentTimeProvider = currentTimeProvider;
        }

        public Task Handle(RequestTimeout<TMessage> sideEffect, CancellationToken cancellationToken = default)
        {
            var dueDate = _currentTimeProvider().Add(sideEffect.TimeSpan);
            _timeoutsManager.NewTimeoutRegistered(dueDate);
            return _timeoutsRepository.Add(new TimeoutRecord(sideEffect.InstanceId, dueDate, sideEffect.Message, typeof(TMessage)));
        }
    }
}
