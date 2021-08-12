// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Effects;
using Unit = NBB.Core.Effects.Unit;

namespace NBB.Application.MediatR.Effects
{
    public static class MediatorEffects
    {
        public class Send
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

        public class Publish
        {
            public class SideEffect : ISideEffect
            {
                public INotification Notification { get; }

                public SideEffect(INotification notification)
                {
                    Notification = notification;
                }
            }


            public class Handler : ISideEffectHandler<SideEffect, Unit>
            {
                private readonly IMediator _mediator;

                public Handler(IMediator mediator)
                {
                    _mediator = mediator;
                }

                public async Task<Unit> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
                {
                    await _mediator.Publish(sideEffect.Notification, cancellationToken);
                    return Unit.Value;
                }
            }
        }
    }

    public static class Mediator
    {
        public static Effect<TResponse> Send<TResponse>(IRequest<TResponse> query) =>
            Effect.Of<MediatorEffects.Send.SideEffect<TResponse>, TResponse>(
                new MediatorEffects.Send.SideEffect<TResponse>(query));

        public static Effect<Unit> Send(IRequest cmd) =>
            Effect.Of<MediatorEffects.Send.SideEffect<global::MediatR.Unit>, global::MediatR.Unit>(
                new MediatorEffects.Send.SideEffect<global::MediatR.Unit>(cmd)).ToUnit();

        public static Effect<Unit> Publish(INotification notification) =>
            Effect.Of<MediatorEffects.Publish.SideEffect, Unit>(new MediatorEffects.Publish.SideEffect(notification));

    }
}
