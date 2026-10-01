// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.Data.Abstractions
{
    /// <summary>
    /// Publishes the uncommitted changes of the tracked entities through <see cref="IEventPublisher"/>, after the inner unit of work was saved.
    /// </summary>
    public class EventPublishingUowDecorator<TEntity>(IUow<TEntity> inner, IEventPublisher eventPublisher) : IUow<TEntity>
        where TEntity : IEventedEntity
    {
        public IEnumerable<TEntity> GetChanges()
        {
            return inner.GetChanges();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var events = GetChanges().SelectMany(e => e.GetUncommittedChanges().ToList()).ToList();
            await inner.SaveChangesAsync(cancellationToken);
            await eventPublisher.PublishAsync(events, cancellationToken);
        }
    }
}
