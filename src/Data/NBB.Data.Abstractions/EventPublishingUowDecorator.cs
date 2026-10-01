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
    public class EventPublishingUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : IEventedEntity
    {
        private readonly IUow<TEntity> _inner;
        private readonly IEventPublisher _eventPublisher;

        public EventPublishingUowDecorator(IUow<TEntity> inner, IEventPublisher eventPublisher)
        {
            _inner = inner;
            _eventPublisher = eventPublisher;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _inner.GetChanges();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var events = GetChanges().SelectMany(e => e.GetUncommittedChanges().ToList()).ToList();
            await _inner.SaveChangesAsync(cancellationToken);
            await _eventPublisher.PublishAsync(events, cancellationToken);
        }
    }
}
