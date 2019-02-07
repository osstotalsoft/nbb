using System;
using System.Collections.Generic;
using NBB.Core.Abstractions;
using NBB.Domain.Abstractions;

namespace NBB.Domain
{
    public abstract class EventedAggregateRoot<TIdentity> : Entity<TIdentity>, IEventedAggregateRoot<TIdentity>
    {
        private readonly List<IDomainEvent> _changes = new List<IDomainEvent>();
        public int Version { get; internal set; }

        protected void AddEvent(IDomainEvent @event)
        {
            _changes.Add(@event);
            @event.SequenceNumber = Version + _changes.Count;
        }


        public IEnumerable<IEvent> GetUncommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            Version += _changes.Count;
            _changes.Clear();
        }
    }
}
