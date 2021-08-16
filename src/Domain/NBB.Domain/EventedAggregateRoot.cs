// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using NBB.Domain.Abstractions;

namespace NBB.Domain
{
    public abstract class EventedAggregateRoot<TIdentity> : Entity<TIdentity>, IEventedAggregateRoot<TIdentity>
    {
        private readonly List<object> _changes = new();
        public int Version { get; internal set; }

        protected void AddEvent(object @event)
        {
            _changes.Add(@event);
        }

        public IEnumerable<object> GetUncommittedChanges()
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