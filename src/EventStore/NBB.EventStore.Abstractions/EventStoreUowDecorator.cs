using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

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

        public async Task SaveChangesAsync()
        {
            var streams = this.GetChanges()
                .Select(e => new {Stream = e.GetStream(), Events = e.GetUncommittedChanges().ToList()}).ToList();
            
            await _inner.SaveChangesAsync();
            await OnAfterSave(streams);
        }

        private async Task OnAfterSave(IEnumerable<dynamic> changes)
        {
            foreach (var @entity in changes)
            {
                await _eventStore.AppendEventsToStreamAsync(@entity.Stream, @entity.Events, null);
            }
        }
    }
}
