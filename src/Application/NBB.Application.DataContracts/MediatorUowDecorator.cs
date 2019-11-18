using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            var events = this.GetChanges().SelectMany(e => e.GetUncommittedChanges().ToList()).ToList();
            await _inner.SaveChangesAsync(cancellationToken);
            await OnAfterSave(events, cancellationToken);
        }

        private async Task OnAfterSave(List<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events.OfType<INotification>())
            {
                await _mediator.Publish(@event, cancellationToken);
            }
        }
    }
}
