using NBB.Core.Abstractions.Paging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.Abstractions
{
    public interface IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken, params string[] includePaths);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken, params string[] includePaths);
        Task<PagedResult<TEntity>> GetAllPagedAsync(PageRequest pageRequest, CancellationToken cancellationToken, params string[] includePaths);
    }
}