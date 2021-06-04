using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEntityTypeConfigurationDecorator<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        private readonly IEntityTypeConfiguration<TEntity> _innerConfiguration;
        private readonly IServiceProvider _sp;
        private readonly DbContext _dbContext;
        private readonly bool _isMultiTenant;
        private string _tenantIdColumn;

        public MultiTenantEntityTypeConfigurationDecorator(IEntityTypeConfiguration<TEntity> innerConfiguration, IServiceProvider sp, DbContext dbContext)
        {
            _innerConfiguration = innerConfiguration;
            _sp = sp;
            _dbContext = dbContext;
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

            var isSharedDb = IsSharedDb();

            builder.HasAnnotation(MultiTenancy.MultiTenantAnnotation, true);
            AddTenantIdProperty(builder);
            AddTenantIdQueryFilter(builder, isSharedDb);
        }

        private bool IsSharedDb()
        {
            var tenantId = _dbContext.GetTenantIdFromContext();

            var tenantDatabaseConfigService = _sp.GetRequiredService<ITenantDatabaseConfigService>();
            return tenantDatabaseConfigService.IsSharedDatabase(tenantId);
        }

        private void AddTenantIdQueryFilter(EntityTypeBuilder<TEntity> builder, bool isSharedDb)
        {
            if (!isSharedDb)
            {
                return;
            }

            https://github.com/dotnet/efcore/pull/11017
            builder.HasQueryFilter(t =>
                EF.Property<Guid>(t, MultiTenancy.TenantIdProp) == _dbContext.GetTenantIdFromContext());
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