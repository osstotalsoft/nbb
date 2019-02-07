namespace NBB.Domain.Abstractions
{
    public interface IAggregateRoot : IEntity
    {

    }

    public interface IAggregateRoot<out TIdentity> : IAggregateRoot, IEntity<TIdentity>
    {

    }
}
