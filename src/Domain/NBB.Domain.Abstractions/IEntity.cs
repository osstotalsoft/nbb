// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;

namespace NBB.Domain.Abstractions
{
    public interface IEntity : IIdentifiedEntity
    {
    }

    public interface IEntity<out TIdentity> : IEntity
    {
        new TIdentity GetIdentityValue();
    }
}
