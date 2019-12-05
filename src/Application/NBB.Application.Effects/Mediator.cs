using NBB.Core.Abstractions;
using NBB.Core.Effects;

namespace NBB.Application.Effects
{
    public static class Mediator
    {
        public static IEffect<TResponse> SendQuery<TResponse>(IQuery<TResponse> query) => Effect.Of(new MediatorSendQuery.SideEffect<TResponse>(query));
    }
}
