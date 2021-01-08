using NBB.Core.Abstractions;
using NBB.Core.Effects;

namespace NBB.Application.Effects
{
    public static class Mediator
    {
        public static Effect<TResponse> SendQuery<TResponse>(IQuery<TResponse> query) => Effect.Of<MediatorSendQuery.SideEffect<TResponse>, TResponse>(new MediatorSendQuery.SideEffect<TResponse>(query));
    }
}
