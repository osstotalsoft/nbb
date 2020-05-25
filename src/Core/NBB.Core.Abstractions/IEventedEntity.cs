using System.Collections.Generic;

namespace NBB.Core.Abstractions
{
    public interface IEventedEntity
    {
        IEnumerable<IEvent> GetUncommittedChanges();
        void MarkChangesAsCommitted();
    }
}
