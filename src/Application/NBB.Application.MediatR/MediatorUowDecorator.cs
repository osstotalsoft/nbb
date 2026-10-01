// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using MediatR;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;

namespace NBB.Application.MediatR
{
    [Obsolete("Use EventPublishingUowDecorator<TEntity> (NBB.Data.Abstractions) together with AddMediatRIntegration(). Will be removed in NBB 11.")]
    public class MediatorUowDecorator<TEntity>(IUow<TEntity> inner, IMediator mediator)
        : EventPublishingUowDecorator<TEntity>(inner, new MediatREventPublisher(mediator))
        where TEntity : IEventedEntity;
}
