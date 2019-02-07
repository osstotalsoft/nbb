using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBB.Data.Abstractions
{
    public interface ICrudRepository<TEntity> : IReadOnlyRepository<TEntity>, IUowRepository<TEntity>
        where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task Update(TEntity entity);
        Task Remove(object id);
    }
}
