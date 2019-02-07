using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.Core.Abstractions
{
    public interface IEventedEntity
    {
        IEnumerable<IEvent> GetUncommittedChanges();
        void MarkChangesAsCommitted();
    }
}
