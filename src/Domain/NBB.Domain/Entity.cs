using System;
using System.Collections.Generic;
using NBB.Core.Abstractions;
using NBB.Domain.Abstractions;

namespace NBB.Domain
{
    public abstract class Entity<TIdentity> : IEntity<TIdentity>
    {
        int? _requestedHashCode;

        public abstract TIdentity GetIdentityValue();
        object IIdentifiedEntity.GetIdentityValue() => GetIdentityValue();

        public bool IsTransient()
        {
            return EqualityComparer<TIdentity>.Default.Equals(this.GetIdentityValue(), default(TIdentity));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Entity<TIdentity>))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            Entity<TIdentity> item = (Entity<TIdentity>)obj;

            if (item.IsTransient() || this.IsTransient())
                return false;
            else
                return EqualityComparer<TIdentity>.Default.Equals(item.GetIdentityValue(), this.GetIdentityValue());
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = this.GetIdentityValue().GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();

        }

        public static bool operator ==(Entity<TIdentity> left, Entity<TIdentity> right)
        {
            if (Object.Equals(left, null))
                return (Object.Equals(right, null)) ? true : false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Entity<TIdentity> left, Entity<TIdentity> right)
        {
            return !(left == right);
        }
    }

    public abstract class Entity<TIdentity, TMemento> : Entity<TIdentity>, IMementoProvider<TMemento>
    {
        void IMementoProvider.SetMemento(object memento) => SetMemento((TMemento)memento) ;
        object IMementoProvider.CreateMemento() => CreateMemento();

        TMemento IMementoProvider<TMemento>.CreateMemento() => CreateMemento();
        void IMementoProvider<TMemento>.SetMemento(TMemento memento) => SetMemento(memento);

        protected abstract void SetMemento(TMemento memento);
        protected abstract TMemento CreateMemento();
    }
}
