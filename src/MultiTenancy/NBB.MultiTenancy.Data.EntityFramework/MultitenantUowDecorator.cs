using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Data.Abstractions;
using NBB.MultiTenancy.Data.EntityFramework.Exceptions;
using NBB.MultiTenancy.Data.EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Data.EntityFramework
{
    public class MultitenantUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : class
    {
        private readonly ITenantService _tenantService;
        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;
        private readonly IUow<TEntity> _inner;

        public MultitenantUowDecorator(IUow<TEntity> inner, ITenantService tenantService, ITenantDatabaseConfigService tenantDatabaseConfigService)
        {

            _tenantDatabaseConfigService = tenantDatabaseConfigService;
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

            var changes = _inner.GetChanges().ToList();

            if (_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                UpdateDefaultTenantId(changes, tenant);
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
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

            if (toCheck.Count == 0)
            {
                return;
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
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
            var mandatoryIds = (from e in changes
                                where e is IMustHaveTenant
                                select ((IMustHaveTenant)e).TenantId)
                       .Distinct()
                       .ToList();

            var toCheck = mandatoryIds;
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