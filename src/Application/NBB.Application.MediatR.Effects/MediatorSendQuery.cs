using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Effects;

namespace NBB.Application.MediatR.Effects
{
    public class MediatorSendQuery
    {
        public class SideEffect<TResponse> : ISideEffect<TResponse>, IAmHandledBy<Handler<TResponse>>
        {
            public IRequest<TResponse> Query { get; }

            public SideEffect(IRequest<TResponse> query)
            {
                Query = query;
            }
        }


        public class Handler<TResponse> : ISideEffectHandler<SideEffect<TResponse>, TResponse>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task<TResponse> Handle(SideEffect<TResponse> sideEffect, CancellationToken cancellationToken = default)
            {
                return _mediator.Send(sideEffect.Query, cancellationToken);
            }
        }
    }
}
