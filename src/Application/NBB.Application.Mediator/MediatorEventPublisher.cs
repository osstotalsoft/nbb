// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using NBB.Core.Abstractions;

namespace NBB.Application.Mediator
{
    /// <summary>
    /// Publishes the events that are Mediator notifications, sequentially, to the Mediator notification handlers.
    /// </summary>
    public class MediatorEventPublisher(IPublisher publisher) : IEventPublisher
    {
        public async Task PublishAsync(IEnumerable<object> events, CancellationToken cancellationToken = default)
        {
            foreach (var @event in events.OfType<INotification>())
            {
                await publisher.Publish(@event, cancellationToken);
            }
        }
    }
}
