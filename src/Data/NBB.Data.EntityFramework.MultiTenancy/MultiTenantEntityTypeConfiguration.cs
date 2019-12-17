using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        private readonly ITenantService _tenantService;
        public MultiTenantEntityTypeConfiguration(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.IsMultiTenant();
        }

        public void AddQueryFilter<T>(ModelBuilder modelBuilder) where T : class
        {
            Expression<Func<T, bool>> filter = t => EF.Property<Guid>(t, "TenantId") == _tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult().TenantId;
            modelBuilder.Entity<T>().HasQueryFilter((LambdaExpression)filter);
        }

        public void AddQueryFilters(ModelBuilder modelBuilder, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            var entities = modelBuilder.Model.GetEntityTypes().Where(p => p.ClrType.GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0).ToList();

            entities.ToList().ForEach(t =>
            {
                var method = this.GetType().GetMethod("AddQueryFilter");
                var genericMethod = method.MakeGenericMethod(t.ClrType);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            });
        }

        private static void AddTenantProperty<T>(ModelBuilder modelBuilder, T entity) where T : class
        {
            modelBuilder.Entity<T>()
                .Property<Guid>("TenantId");
        }

        public void AddTenantIdProperties(ModelBuilder modelBuilder, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            var mandatory = new List<IMutableEntityType>();

            var entityTypes = modelBuilder.Model.GetEntityTypes();
            var listByAttributes = modelBuilder.Model.GetEntityTypes().Where(p => p.ClrType.GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0).ToList();

            var listByAnnotations = modelBuilder.Model.GetEntityTypes().Where(e => e.IsMultiTenant())
                .Distinct()
                .ToList();

            var listOfTenantAware = listByAnnotations.Union(listByAttributes).Distinct().ToList();
            foreach (var entity in listOfTenantAware)
            {
                var e = modelBuilder.Entity(entity.ClrType);
                e.Property(typeof(Guid), "TenantId");
            }
        }
    }
}