// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
            => EqualityComparer<TIdentity>.Default.Equals(GetIdentityValue(), default);

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not Entity<TIdentity>)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            Entity<TIdentity> item = (Entity<TIdentity>)obj;

            if (item.IsTransient() || this.IsTransient())
                return false;

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

            return base.GetHashCode();

        }

        public static bool operator ==(Entity<TIdentity> left, Entity<TIdentity> right)
            => left?.Equals(right) ?? Equals(right, null);

        public static bool operator !=(Entity<TIdentity> left, Entity<TIdentity> right)
            => !(left == right);
    }

    public abstract class Entity<TIdentity, TMemento> : Entity<TIdentity>, IMementoProvider<TMemento>
    {
        void IMementoProvider.SetMemento(object memento) => SetMemento((TMemento)memento);
        object IMementoProvider.CreateMemento() => CreateMemento();

        TMemento IMementoProvider<TMemento>.CreateMemento() => CreateMemento();
        void IMementoProvider<TMemento>.SetMemento(TMemento memento) => SetMemento(memento);

        protected abstract void SetMemento(TMemento memento);
        protected abstract TMemento CreateMemento();
    }
}
