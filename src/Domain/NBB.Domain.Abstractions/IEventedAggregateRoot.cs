// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;

namespace NBB.Domain.Abstractions
{
    public interface IEventedAggregateRoot : IAggregateRoot, IEventedEntity
    {
    }

    public interface IEventedAggregateRoot<out TIdentity> : IEventedAggregateRoot, IAggregateRoot<TIdentity>
    {
    }
}
