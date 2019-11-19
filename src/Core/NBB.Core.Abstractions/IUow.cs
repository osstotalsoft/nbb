using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Abstractions
{
    public interface IUow<out TEntity>
    {
        IEnumerable<TEntity> GetChanges();
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
