using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.Abstractions
{
    public interface ICrudRepository<TEntity> : IReadOnlyRepository<TEntity>, IUowRepository<TEntity>
        where TEntity : class
    {
        Task AddAsync(TEntity entity, CancellationToken cancellationToken);
        Task Update(TEntity entity, CancellationToken cancellationToken);
        Task Remove(object id, CancellationToken cancellationToken);
    }
}
