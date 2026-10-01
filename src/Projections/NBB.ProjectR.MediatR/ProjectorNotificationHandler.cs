// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProjectR.MediatR
{
    internal sealed class ProjectorNotificationHandler<TEvent, TModel, TMessage, TIdentity>(ProjectorEventProcessor<TEvent, TModel, TMessage, TIdentity> processor) : INotificationHandler<TEvent>
        where TEvent : INotification
    {
        public Task Handle(TEvent notification, CancellationToken cancellationToken)
            => processor.Handle(notification, cancellationToken);
    }
}
