// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Abstractions;

namespace NBB.Application.MediatR
{
    /// <summary>
    /// Publishes the events that are MediatR notifications, sequentially, to the MediatR notification handlers.
    /// </summary>
    public class MediatREventPublisher(IPublisher publisher) : IEventPublisher
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
