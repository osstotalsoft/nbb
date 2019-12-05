using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Abstractions;
using NBB.Core.Effects;

namespace NBB.Application.Effects
{
    public class MediatorSendQuery
    {
        public class SideEffect<TResponse> : ISideEffect<TResponse>, IAmHandledBy<Handler<TResponse>>
        {
            public IQuery<TResponse> Query { get; }

            public SideEffect(IQuery<TResponse> query)
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
                return _mediator.Send(sideEffect.Query as IRequest<TResponse>, cancellationToken);
            }
        }
    }
}
