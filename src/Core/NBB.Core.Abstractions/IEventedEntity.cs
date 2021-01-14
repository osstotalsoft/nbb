using System.Collections.Generic;

namespace NBB.Core.Abstractions
{
    public interface IEventedEntity
    {
        IEnumerable<object> GetUncommittedChanges();
        void MarkChangesAsCommitted();
    }
}
