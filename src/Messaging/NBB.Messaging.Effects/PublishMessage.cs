using System.Threading.Tasks;
using NBB.Core.Effects;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Effects
{
    public class PublishMessage
    {
        public class SideEffect : ISideEffect<int>
        {
            public object Message { get; }

            public SideEffect(object message)
            {
                Message = message;
            }
        }

        public class Handler : ISideEffectHandler<SideEffect, int>
        {
            private readonly IMessageBusPublisher _messageBusPublisher;

            public Handler(IMessageBusPublisher messageBusPublisher)
            {
                _messageBusPublisher = messageBusPublisher;
            }

            public async Task<int> Handle(SideEffect sideEffect)
            {
                await _messageBusPublisher.PublishAsync(sideEffect.Message as dynamic);
                return 0;
            }
        }
    }
}
