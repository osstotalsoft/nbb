// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Abstractions
{
    public class EventStoreUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : IEventedEntity, IIdentifiedEntity
    {
        private readonly IUow<TEntity> _inner;
        private readonly IEventStore _eventStore;

        public EventStoreUowDecorator(IUow<TEntity> inner, IEventStore eventStore)
        {
            _inner = inner;
            _eventStore = eventStore;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _inner.GetChanges();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var streams = this.GetChanges()
                .Select(e => new {Stream = e.GetStream(), Events = e.GetUncommittedChanges().ToList()}).ToList();
            
            await _inner.SaveChangesAsync(cancellationToken);
            await OnAfterSave(streams, cancellationToken);
        }

        private async Task OnAfterSave(IEnumerable<dynamic> changes, CancellationToken cancellationToken = default)
        {
            foreach (var @entity in changes)
            {
                await _eventStore.AppendEventsToStreamAsync(@entity.Stream, @entity.Events, null, cancellationToken);
            }
        }
    }
}
