using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Effects
{
    public class PublishMessage
    {
        public class SideEffect : ISideEffect
        {
            public object Message { get; }

            public SideEffect(object message)
            {
                Message = message;
            }
        }

        public class Handler : ISideEffectHandler<SideEffect, Unit>
        {
            private readonly IMessageBusPublisher _messageBusPublisher;

            public Handler(IMessageBusPublisher messageBusPublisher)
            {
                _messageBusPublisher = messageBusPublisher;
            }

            public async Task<Unit> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
            {
                await _messageBusPublisher.PublishAsync(sideEffect.Message as dynamic, cancellationToken);
                return Unit.Value;
            }
        }
    }
}
