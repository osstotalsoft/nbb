// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Domain.Abstractions
{
    public interface IAggregateRoot : IEntity
    {

    }

    public interface IAggregateRoot<out TIdentity> : IAggregateRoot, IEntity<TIdentity>
    {

    }
}
