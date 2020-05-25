using NBB.Domain.Abstractions;
using ReflectionMagic;

namespace NBB.Domain
{
    public abstract class EmitApplyAggregateRoot<TIdentity> : EventedAggregateRoot<TIdentity>
    {
        

        //https://github.com/d60/Cirqus/wiki/Emit-Apply-Pattern
        protected void Emit(IDomainEvent @event)
        {
            ApplyChanges(@event, true);
        }

        // push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
        internal void ApplyChanges(IDomainEvent @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);
            if (isNew)
            {
                AddEvent(@event);
            }
            else
            {
                Version++;
            }


        }

    }
}
