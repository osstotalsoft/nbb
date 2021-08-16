// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;

namespace NBB.Core.Abstractions
{
    public interface IEventedEntity
    {
        IEnumerable<object> GetUncommittedChanges();
        void MarkChangesAsCommitted();
    }
}
