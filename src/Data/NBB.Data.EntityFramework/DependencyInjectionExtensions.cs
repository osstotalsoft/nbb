using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Data.EntityFramework.Internal;

namespace NBB.Data.EntityFramework
{
    public static class DependencyInjectionExtensions
    {
        public static void AddEntityFrameworkDataAccess(this IServiceCollection services)
        {
            services.AddSingleton<IExpressionBuilder, ExpressionBuilder>();
        }

        public static void AddEfCrudRepository<TEntity, TContext>(this IServiceCollection services)
            where TEntity : class
            where TContext : DbContext
        {
            services.AddScoped<ICrudRepository<TEntity>, EfCrudRepository<TEntity, TContext>>();
            services.AddEfUow<TEntity, TContext>();
        }

        public static void AddEfReadOnlyRepository<TEntity, TContext>(this IServiceCollection services)
            where TEntity : class
            where TContext : DbContext
        {
            services.AddScoped<IReadOnlyRepository<TEntity>, EfReadOnlyRepository<TEntity, TContext>>();
        }

        public static void AddEfQuery<TEntity, TContext>(this IServiceCollection services)
            where TEntity : class
            where TContext : DbContext
        {
            services.AddScoped<IQueryable<TEntity>, EfQuery<TEntity, TContext>>();
        }

        public static void AddEfUow<TEntity, TContext>(this IServiceCollection services)
            where TEntity : class
            where TContext : DbContext
        {
            services.AddScoped<IUow<TEntity>, EfUow<TEntity, TContext>>();
        }
    }
}
