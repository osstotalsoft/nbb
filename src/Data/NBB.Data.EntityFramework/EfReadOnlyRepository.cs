using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions.Paging;
using NBB.Data.Abstractions;
using NBB.Data.EntityFramework.Internal;

namespace NBB.Data.EntityFramework
{
    public class EfReadOnlyRepository<TEntity, TContext> : IReadOnlyRepository<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly TContext _c;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly ILogger<EfReadOnlyRepository<TEntity, TContext>> _logger;

        public  EfReadOnlyRepository(TContext c, IExpressionBuilder expressionBuilder, ILogger<EfReadOnlyRepository<TEntity, TContext>> logger)
        {
            _c = c;
            _c.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _expressionBuilder = expressionBuilder;
            _logger = logger;
        }

        public async Task<TEntity> GetByIdAsync(object id, params string[] includePaths)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var lambda = _expressionBuilder.BuildPrimaryKeyExpressionFromModel<TEntity>(_c.Model, id);
            var result = await _c.Set<TEntity>().IncludePaths(includePaths).FirstOrDefaultAsync(lambda);

            stopWatch.Stop();
            _logger.LogDebug("EfReadRepository.GetByIdAsync for {EntityType} with {IncludePaths} took {ElapsedMilliseconds} ms", typeof(TEntity).Name, string.Join(", ", includePaths), stopWatch.ElapsedMilliseconds);

            return result;
        }


        public async Task<IEnumerable<TEntity>> GetAllAsync(params string[] includePaths)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var results = await _c.Set<TEntity>().IncludePaths(includePaths)
                .ToListAsync();

            stopWatch.Stop();
            _logger.LogDebug("EfReadRepository.GetAllAsync for {EntityType} with {IncludePaths} took {ElapsedMilliseconds} ms", typeof(TEntity).Name, string.Join(", ", includePaths), stopWatch.ElapsedMilliseconds);

            return results;
        }

        public async Task<PagedResult<TEntity>> GetAllPagedAsync(PageRequest pageRequest, params string[] includePaths)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var keyProperties = _c.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties;
            var id = keyProperties.First().Name;                
            
            var results = await _c.Set<TEntity>().IncludePaths(includePaths)                
                .OrderBy(id, false)
                .ToPagedResult(pageRequest);

            stopWatch.Stop();
            _logger.LogDebug("EfReadRepository.GetAllPagedAsync for {EntityType} with page {Page}, page size {PageSize} and {IncludePaths} took {ElapsedMilliseconds} ms", 
                typeof(TEntity).Name, pageRequest.Page, pageRequest.PageSize, string.Join(", ", includePaths), stopWatch.ElapsedMilliseconds);

            return results;            
        }

    }

    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> IncludePaths<TEntity>(this IQueryable<TEntity> query, params string[] includePaths)
             where TEntity : class
        {
            if (includePaths != null)
            {
                foreach (var includePath in includePaths)
                    query = query.Include(includePath);
            }

            return query;
        }

        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty,
            bool desc) 
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }
    }
}
