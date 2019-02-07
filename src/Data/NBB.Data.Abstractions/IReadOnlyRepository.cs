using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NBB.Core.Abstractions.Paging;

namespace NBB.Data.Abstractions
{
    public interface IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object id, params string[] includePaths);
        Task<IEnumerable<TEntity>> GetAllAsync(params string[] includePaths);
        Task<PagedResult<TEntity>> GetAllPagedAsync(PageRequest pageRequest, params string[] includePaths);
    }
}
