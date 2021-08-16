// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Data.EntityFramework;
using NBB.Data.EntityFramework.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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

        public static void AddEfAsyncEnumerable<TEntity, TContext>(this IServiceCollection services)
            where TEntity : class
            where TContext : DbContext
        {
            services.AddScoped<IAsyncEnumerable<TEntity>, EfQuery<TEntity, TContext>>();
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
