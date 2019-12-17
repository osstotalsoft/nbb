using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEntityTypeConfigurationDecorator<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        private readonly IEntityTypeConfiguration<TEntity> _innerConfiguration;
        private readonly IServiceProvider _sp;
        private string _tenantIdColumn;

        public MultiTenantEntityTypeConfigurationDecorator(IEntityTypeConfiguration<TEntity> innerConfiguration, IServiceProvider sp)
        {
            _innerConfiguration = innerConfiguration;
            _sp = sp;
        }

        public MultiTenantEntityTypeConfigurationDecorator<TEntity> WithTenantIdColumn(string tenantIdColumn)
        {
            _tenantIdColumn = tenantIdColumn;
            return this;
        }

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            _innerConfiguration.Configure(builder);


            var tenantId = ResolveTenantId();
            var isSharedDb = IsSharedDb(tenantId);
            
            builder.HasAnnotation(MultiTenancy.MultiTenantAnnotation, true);
            AddTenantIdProperty(builder);
            AddTenantIdQueryFilter(builder, tenantId, isSharedDb);
        }

        private Guid ResolveTenantId()
        {
            var tenantService = _sp.GetRequiredService<ITenantService>();
            var tenant = tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult();
            if (tenant == null)
            {
                throw new Exception("Tenant not available");
            }

            return tenant.TenantId;
        }

        private bool IsSharedDb(Guid tenantId)
        {
            var tenantDatabaseConfigService = _sp.GetRequiredService<ITenantDatabaseConfigService>();
            return tenantDatabaseConfigService.IsSharedDatabase(tenantId);
        }

        private void AddTenantIdQueryFilter(EntityTypeBuilder<TEntity> builder, Guid tenantId, bool isSharedDb)
        {
            if (isSharedDb)
            {
                return;
            }

            Expression<Func<TEntity, bool>> filter = t =>
                EF.Property<Guid?>(t, MultiTenancy.TenantIdProp) == tenantId;
            builder.HasQueryFilter(filter);
        }

        private void AddTenantIdProperty(EntityTypeBuilder<TEntity> builder)
        {
            var prop = builder.Property<Guid?>(MultiTenancy.TenantIdProp);
            if (!string.IsNullOrWhiteSpace(_tenantIdColumn))
            {
                prop.HasColumnName(_tenantIdColumn);
            }
        }
    }
}
