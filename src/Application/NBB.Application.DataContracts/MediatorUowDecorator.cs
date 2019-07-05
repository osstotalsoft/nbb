using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Abstractions;

namespace NBB.Application.DataContracts
{
    public class MediatorUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : IEventedEntity
    {
        private readonly IUow<TEntity> _inner;
        private readonly IMediator _mediator;

        public MediatorUowDecorator(IUow<TEntity> inner, IMediator mediator)
        {
            _inner = inner;
            _mediator = mediator;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _inner.GetChanges();
        }

        public async Task SaveChangesAsync()
        {
            var events = this.GetChanges().SelectMany(e => e.GetUncommittedChanges().ToList()).ToList();
            await _inner.SaveChangesAsync();
            await OnAfterSave(events);
        }

        private async Task OnAfterSave(List<IEvent> events)
        {
            foreach (var @event in events.OfType<INotification>())
            {
                await _mediator.Publish(@event);
            }
        }
    }
}
