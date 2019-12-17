using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEntityTypeConfigurationAdapter<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        

        private readonly IEntityTypeConfiguration<TEntity> _innerConfiguration;
        private readonly IServiceProvider _sp;

        public MultiTenantEntityTypeConfigurationAdapter(IEntityTypeConfiguration<TEntity> innerConfiguration, IServiceProvider sp)
        {
            _innerConfiguration = innerConfiguration;
            _sp = sp;
        }

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            _innerConfiguration.Configure(builder);


            var tenantId = ResolveTenantId();
            
            builder.HasAnnotation(MultiTenancy.MultiTenantAnnotation, true);
            AddTenantIdProperty(builder);
            var tenantService = _sp.GetRequiredService<ITenantService>();
            AddTenantIdQueryFilter(builder, tenantId);
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

        private void AddTenantIdQueryFilter(EntityTypeBuilder<TEntity> builder, Guid tenantId)
        {
            Expression<Func<TEntity, bool>> filter = t => EF.Property<Guid?>(t, MultiTenancy.TenantIdProp) == tenantId;
            builder.HasQueryFilter(filter);
        }

        private static void AddTenantIdProperty(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property<Guid?>(MultiTenancy.TenantIdProp);
        }
    }
}
