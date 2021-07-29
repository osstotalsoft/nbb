using System;
using NBB.Core.Effects;
using MediatR;

namespace NBB.ProjectR
{
    public static class Eff
    {
        public static Effect<IMessage<TProjection>> None<TProjection>() => Effect.Pure<IMessage<TProjection>>(null);
    }
    
    public static class MessageBus
    {
        public static Effect<IMessage<TProjection>> PublishEvent<TProjection>(IEvent<TProjection> ev)
            => Messaging.Effects.MessageBus.Publish(ev).Then(Eff.None<TProjection>());
    }

    public static class Mediator
    {
        public static Effect<IMessage<TProjection>> Send<TProjection, TResponse>(IRequest<TResponse> query, Func<TResponse, IMessage<TProjection>> next = null)
        {
            var eff = Application.MediatR.Effects.Mediator.Send(query);
            return next != null ? eff.Then(next) : eff.Then(_ => Eff.None<TProjection>());
        }
    }

}
