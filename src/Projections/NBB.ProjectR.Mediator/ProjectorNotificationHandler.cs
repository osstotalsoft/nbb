// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Mediator;

namespace NBB.ProjectR.Mediator
{
    /// <summary>
    /// Bridges Mediator notifications to a projector. Internal and generic on purpose:
    /// the Mediator source generator skips it, it is registered in DI by AddProjectRMediatorHandlers().
    /// </summary>
    internal sealed class ProjectorNotificationHandler<TEvent, TModel, TMessage, TIdentity>(ProjectorEventProcessor<TEvent, TModel, TMessage, TIdentity> processor) : INotificationHandler<TEvent>
        where TEvent : INotification
    {
        public ValueTask Handle(TEvent notification, CancellationToken cancellationToken)
            => new(processor.Handle(notification, cancellationToken));
    }
}
