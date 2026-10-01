// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using NBB.Core.Effects;
using Unit = NBB.Core.Effects.Unit;

namespace NBB.Application.Mediator.Effects
{
    public static class MediatorEffects
    {
        public class Send
        {
            public class RequestSideEffect<TResponse> : ISideEffect<TResponse>, IAmHandledBy<RequestHandler<TResponse>>
            {
                public IMessage Request { get; }

                public RequestSideEffect(IMessage request)
                {
                    Request = request;
                }
            }

            public class RequestHandler<TResponse>(ISender sender) : ISideEffectHandler<RequestSideEffect<TResponse>, TResponse>
            {
                public Task<TResponse> Handle(RequestSideEffect<TResponse> sideEffect, CancellationToken cancellationToken = default)
                    => sideEffect.Request switch
                    {
                        IRequest<TResponse> request => sender.Send(request, cancellationToken).AsTask(),
                        ICommand<TResponse> command => sender.Send(command, cancellationToken).AsTask(),
                        IQuery<TResponse> query => sender.Send(query, cancellationToken).AsTask(),
                        _ => throw new InvalidOperationException(
                            $"{sideEffect.Request.GetType()} is not a Mediator request, command or query with response {typeof(TResponse)}")
                    };
            }
        }

        public class Publish
        {
            public class SideEffect : ISideEffect, IAmHandledBy<Handler>
            {
                public INotification Notification { get; }

                public SideEffect(INotification notification)
                {
                    Notification = notification;
                }
            }

            public class Handler(IPublisher publisher) : ISideEffectHandler<SideEffect, Unit>
            {
                public async Task<Unit> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
                {
                    await publisher.Publish(sideEffect.Notification, cancellationToken);
                    return Unit.Value;
                }
            }
        }
    }

    /// <summary>
    /// Effects that send requests and publish notifications through Mediator. Requires AddMediatorEffects().
    /// </summary>
    public static class MediatorEff
    {
        public static Effect<TResponse> Send<TResponse>(IRequest<TResponse> request) => SendMessage<TResponse>(request);

        public static Effect<TResponse> Send<TResponse>(ICommand<TResponse> command) => SendMessage<TResponse>(command);

        public static Effect<TResponse> Send<TResponse>(IQuery<TResponse> query) => SendMessage<TResponse>(query);

        public static Effect<Unit> Send(IRequest request) => SendMessage<global::Mediator.Unit>(request).ToUnit();

        public static Effect<Unit> Send(ICommand command) => SendMessage<global::Mediator.Unit>(command).ToUnit();

        public static Effect<Unit> Publish(INotification notification) =>
            Effect.Of<MediatorEffects.Publish.SideEffect, Unit>(new MediatorEffects.Publish.SideEffect(notification));

        private static Effect<TResponse> SendMessage<TResponse>(IMessage message) =>
            Effect.Of<MediatorEffects.Send.RequestSideEffect<TResponse>, TResponse>(
                new MediatorEffects.Send.RequestSideEffect<TResponse>(message));
    }
}
