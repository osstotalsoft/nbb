using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEntityTypeConfigurationDecorator<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        private readonly IEntityTypeConfiguration<TEntity> _innerConfiguration;
        private readonly IServiceProvider _sp;
        private readonly bool _isMultiTenant;
        private string _tenantIdColumn;

        public MultiTenantEntityTypeConfigurationDecorator(IEntityTypeConfiguration<TEntity> innerConfiguration, IServiceProvider sp)
        {
            _innerConfiguration = innerConfiguration;
            _sp = sp;

            var tenancyOptions = _sp.GetRequiredService<IOptions<TenancyHostingOptions>>();
            _isMultiTenant = tenancyOptions?.Value?.TenancyType != TenancyType.None;
        }

        public MultiTenantEntityTypeConfigurationDecorator<TEntity> WithTenantIdColumn(string tenantIdColumn)
        {
            _tenantIdColumn = tenantIdColumn;
            return this;
        }

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            _innerConfiguration.Configure(builder);

            if (!_isMultiTenant)
                return;

            var tenantId = ResolveTenantId();
            var isSharedDb = IsSharedDb(tenantId);

            builder.HasAnnotation(MultiTenancy.MultiTenantAnnotation, true);
            AddTenantIdProperty(builder);
            AddTenantIdQueryFilter(builder, tenantId, isSharedDb);
        }

        private Guid ResolveTenantId()
        {
            var tenantContextAccessor = _sp.GetRequiredService<ITenantContextAccessor>();
            var tenantId = tenantContextAccessor.TenantContext.GetTenantId();

            return tenantId;
        }

        private bool IsSharedDb(Guid tenantId)
        {
            var tenantDatabaseConfigService = _sp.GetRequiredService<ITenantDatabaseConfigService>();
            return tenantDatabaseConfigService.IsSharedDatabase(tenantId);
        }

        private void AddTenantIdQueryFilter(EntityTypeBuilder<TEntity> builder, Guid tenantId, bool isSharedDb)
        {
            if (!isSharedDb)
            {
                return;
            }

            Expression<Func<TEntity, bool>> filter = t =>
                EF.Property<Guid>(t, MultiTenancy.TenantIdProp) == tenantId;
            builder.HasQueryFilter(filter);
        }

        private void AddTenantIdProperty(EntityTypeBuilder<TEntity> builder)
        {
            var prop = builder.Property<Guid>(MultiTenancy.TenantIdProp);
            if (!string.IsNullOrWhiteSpace(_tenantIdColumn))
            {
                prop.HasColumnName(_tenantIdColumn);
            }
        }
    }
}