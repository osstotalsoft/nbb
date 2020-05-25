using System.Collections.Generic;

namespace NBB.Domain.Abstractions
{
    public interface IEventSourcedAggregateRoot : IEventedAggregateRoot
    {
        int Version { get; }
        void LoadFromHistory(IEnumerable<IDomainEvent> history);

    }

    public interface IEventSourcedAggregateRoot<out TIdentity> : IEventSourcedAggregateRoot, IEventedAggregateRoot<TIdentity>
    {
    }
}
