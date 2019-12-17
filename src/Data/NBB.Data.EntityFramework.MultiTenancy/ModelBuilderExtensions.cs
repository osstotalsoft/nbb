using System;
using Microsoft.EntityFrameworkCore;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyMultiTenantConfiguration<TEntity>(this ModelBuilder modelBuilder,  IEntityTypeConfiguration<TEntity> configuration, IServiceProvider sp)
            where TEntity : class
        {
            var config = new MultiTenantEntityTypeConfigurationAdapter<TEntity>(configuration, sp);
            modelBuilder.ApplyConfiguration(config);
        }
    }
}
