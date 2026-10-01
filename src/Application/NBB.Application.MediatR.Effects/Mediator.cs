// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
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
            public class QuerySideEffect<TResponse>(IRequest<TResponse> query) : ISideEffect<TResponse>, IAmHandledBy<QueryHandler<TResponse>>
            {
                public IRequest<TResponse> Query { get; } = query;
            }

            public class CommandSideEffect(IRequest query) : ISideEffect<Unit>, IAmHandledBy<CommandHandler>
            {
                public IRequest Query { get; } = query;
            }

            public class QueryHandler<TResponse>(IMediator mediator) : ISideEffectHandler<QuerySideEffect<TResponse>, TResponse>
            {
                public Task<TResponse> Handle(QuerySideEffect<TResponse> sideEffect, CancellationToken cancellationToken = default)
                {
                    return mediator.Send(sideEffect.Query, cancellationToken);
                }
            }

            public class CommandHandler(IMediator mediator) : ISideEffectHandler<CommandSideEffect, Unit>
            {
                public async Task<Unit> Handle(CommandSideEffect sideEffect, CancellationToken cancellationToken = default)
                {
                    await mediator.Send(sideEffect.Query, cancellationToken);

                    return Unit.Value;
                }
            }
        }

        public class Publish
        {
            public class SideEffect(INotification notification) : ISideEffect
            {
                public INotification Notification { get; } = notification;
            }


            public class Handler(IMediator mediator) : ISideEffectHandler<SideEffect, Unit>
            {
                public async Task<Unit> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
                {
                    await mediator.Publish(sideEffect.Notification, cancellationToken);
                    return Unit.Value;
                }
            }
        }
    }

    public static class MediatorEff
    {
        public static Effect<TResponse> Send<TResponse>(IRequest<TResponse> query) =>
            Effect.Of<MediatorEffects.Send.QuerySideEffect<TResponse>, TResponse>(
                new MediatorEffects.Send.QuerySideEffect<TResponse>(query));

        public static Effect<Unit> Send(IRequest cmd) =>
            Effect.Of<MediatorEffects.Send.CommandSideEffect, Unit>(
                new MediatorEffects.Send.CommandSideEffect(cmd)).ToUnit();

        public static Effect<Unit> Publish(INotification notification) =>
            Effect.Of<MediatorEffects.Publish.SideEffect, Unit>(new MediatorEffects.Publish.SideEffect(notification));
    }

    [Obsolete("Use MediatorEff instead. Will be removed in NBB 11.")]
    public static class Mediator
    {
        public static Effect<TResponse> Send<TResponse>(IRequest<TResponse> query) => MediatorEff.Send(query);

        public static Effect<Unit> Send(IRequest cmd) => MediatorEff.Send(cmd);

        public static Effect<Unit> Publish(INotification notification) => MediatorEff.Publish(notification);
    }
}
