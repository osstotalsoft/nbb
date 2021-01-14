using MediatR;
using NBB.Core.Effects;

namespace NBB.Application.MediatR.Effects
{
    public static class Mediator
    {
        public static Effect<TResponse> SendQuery<TResponse>(IRequest<TResponse> query) => Effect.Of<MediatorSendQuery.SideEffect<TResponse>, TResponse>(new MediatorSendQuery.SideEffect<TResponse>(query));
    }
}
