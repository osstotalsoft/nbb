using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyMultiTenantConfiguration<TEntity>(this ModelBuilder modelBuilder, IEntityTypeConfiguration<TEntity> configuration, IServiceProvider sp, DbContext dbContext)
            where TEntity : class
        {
            var config = new MultiTenantEntityTypeConfigurationDecorator<TEntity>(configuration, sp, dbContext);
            modelBuilder.ApplyConfiguration(config);
        }

        public static void ApplyMultiTenantConfiguration<TEntity>(this ModelBuilder modelBuilder, IEntityTypeConfiguration<TEntity> configuration, DbContext dbContext)
            where TEntity : class
        {
            var sp = dbContext.GetInfrastructure();
            modelBuilder.ApplyMultiTenantConfiguration(configuration, sp, dbContext);
        }
    }
}
