using System;
using System.Collections.Generic;
using System.Linq;
using NBB.Core.Abstractions;
using NBB.Domain.Abstractions;

namespace NBB.Domain
{
    public abstract class EventSourcedAggregateRoot<TIdentity> : EmitApplyAggregateRoot<TIdentity>, IEventSourcedAggregateRoot<TIdentity>
    {
        public void LoadFromHistory(IEnumerable<IDomainEvent> history)
        {
            foreach (var domainEvent in history)
            {
                ApplyChanges(domainEvent, false);
            }
        }
    }

    public abstract class EventSourcedAggregateRoot<TIdentity, TMemento> : EventSourcedAggregateRoot<TIdentity>, ISnapshotableEntity, IMementoProvider<TMemento>
    {
        public int SnapshotVersion { get; private set; }

        public virtual int? SnapshotVersionFrequency => null;

        void ISnapshotableEntity.ApplySnapshot(object snapshot, int snapshotVersion)
        {
            var mementoProvider = this as IMementoProvider;

            if (Version > 0) 
                throw new ApplicationException("Cannot apply snapshot on an already loaded aggregate");

            Version = snapshotVersion;
            SnapshotVersion = snapshotVersion;

            mementoProvider.SetMemento(snapshot);
        }

        (object snapshot, int snapshotVersion) ISnapshotableEntity.TakeSnapshot()
        {
            var mementoProvider = this as IMementoProvider;

            var snapshot = mementoProvider.CreateMemento();
            var snapshotVersion =  Version + GetUncommittedChanges().Count();
            SnapshotVersion = snapshotVersion;
            
            return (snapshot, snapshotVersion);
        }

        void IMementoProvider.SetMemento(object memento) => SetMemento((TMemento)memento) ;
        object IMementoProvider.CreateMemento() => CreateMemento();

        TMemento IMementoProvider<TMemento>.CreateMemento() => CreateMemento();
        void IMementoProvider<TMemento>.SetMemento(TMemento memento) => SetMemento(memento);

        protected abstract void SetMemento(TMemento memento);
        protected abstract TMemento CreateMemento();
    }
}
