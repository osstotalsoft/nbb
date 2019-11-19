using Microsoft.EntityFrameworkCore;
using NBB.Core.Abstractions.Paging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework
{
    public static class PagedResultExtensions
    {
        public static async Task<PagedResult<TEntity>> ToPagedResult<TEntity>(this IQueryable<TEntity> query, PageRequest pageRequest, CancellationToken cancellationToken = default)
        {
            int page = pageRequest.Page,
                pageSize = pageRequest.PageSize;

            var totalCount = await query.CountAsync(cancellationToken);
            var values = await query.Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync(cancellationToken);

            var totalPages = totalCount / pageSize + (totalCount % pageSize > 0 ? 1 : 0);

            var result = new PagedResult<TEntity>(page, pageSize, totalCount, totalPages, values);
            return result;
        }
    }
}
