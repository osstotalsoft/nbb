using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Data.Abstractions;
using NBB.MultiTenancy.Data.EntityFramework.Exceptions;
using NBB.MultiTenancy.Data.EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Data.EntityFramework
{
    public class MultitenantUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : class
    {
        private readonly ITenantService _tenantService;
        private readonly TenantDatabaseConfiguration _tenantDatabaseConfiguration;
        private readonly IUow<TEntity> _inner;

        public MultitenantUowDecorator(IUow<TEntity> inner, ITenantService tenantService, TenantDatabaseConfiguration tenantDatabaseConfiguration)
        {

            _tenantDatabaseConfiguration = tenantDatabaseConfiguration;
            _inner = inner;
            _tenantService = tenantService;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var tenant = await _tenantService.GetCurrentTenantAsync();

            if (tenant == null)
            {
                await _inner.SaveChangesAsync();
                return;
            }

            if (_tenantDatabaseConfiguration.IsReadOnly)
            {
                throw new Exception("Readonly");
            }

            var changes = _inner.GetChanges().ToList();

            if (_tenantDatabaseConfiguration.UseDefaultValueOnSave)
            {
                UpdateDefaultTenantId(changes, tenant);
            }

            if (_tenantDatabaseConfiguration.RestrictCrossTenantAccess)
            {
                ThrowIfMultipleTenants(changes, tenant);
            }

            await _inner.SaveChangesAsync();
        }

        protected void ThrowIfMultipleTenants(List<TEntity> changes, Tenant tenant)
        {
            if (tenant == null)
            {
                return;
            }

            var toCheck = GetViolations(changes);

            if (toCheck.Count >= 1 && _tenantDatabaseConfiguration.IsReadOnly)
            {
                throw new Exception("Read only Db context");
            }

            if (toCheck.Count == 0)
            {
                return;
            }

            if (!_tenantDatabaseConfiguration.RestrictCrossTenantAccess)
            {
                return;
            }

            if (toCheck.Count > 1)
            {
                throw new CrossTenantUpdateException(toCheck);
            }

            if (!toCheck.First().Equals(tenant.TenantId))
            {
                throw new CrossTenantUpdateException(toCheck);
            }
        }

        protected List<Guid> GetViolations(List<TEntity> changes)
        {
            var optionalIds = (from e in changes
                               where e is IMayHaveTenant && !((IMayHaveTenant)e).TenantId.IsNullOrDefault()
                               select ((IMayHaveTenant)e).TenantId)
                       .Distinct()
                       .ToList();

            var mandatoryIds = (from e in changes
                                where e is IMustHaveTenant
                                select ((IMustHaveTenant)e).TenantId)
                       .Distinct()
                       .ToList();

            var toCheck = optionalIds.Union(mandatoryIds).ToList();
            return toCheck;
        }

        protected void UpdateDefaultTenantId(List<TEntity> changes, Tenant tenant)
        {
            if (tenant == null)
            {
                return;
            }

            var list = changes
                .Where(e => e is IMustHaveTenant && ((IMustHaveTenant)e).TenantId.IsNullOrDefault())
                .Select(e => ((IMustHaveTenant)e));
            foreach (var e in list)
            {
                e.TenantId = tenant.TenantId;
            }
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _inner.GetChanges();
        }
    }
}
