using System;
using NBB.Core.Effects;
using MediatR;

namespace NBB.ProjectR
{
    public static class Eff
    {
        public static Effect<TMessage> None<TMessage>() => Effect.Pure<TMessage>(default);
    }
    
    //public static class MessageBus
    //{
    //    public static Effect<TMessage> PublishEvent<TMessage>(TMessage ev)
    //        => Messaging.Effects.MessageBus.Publish(ev).Then(Eff.None<TMessage>());
    //}

    //public static class Mediator
    //{
    //    public static Effect<TMessage> Send<TMessage, TResponse>(IRequest<TResponse> query, Func<TResponse, TMessage> next = null)
    //    {
    //        var eff = Application.MediatR.Effects.Mediator.Send(query);
    //        return next != null ? eff.Then(next) : eff.Then(_ => Eff.None<TMessage>());
    //    }
    //}

}
