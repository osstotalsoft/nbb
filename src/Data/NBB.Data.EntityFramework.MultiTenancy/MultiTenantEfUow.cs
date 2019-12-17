using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEfUow<TEntity, TContext> : IUow<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly TContext _c;
        private readonly ILogger<MultiTenantEfUow<TEntity, TContext>> _logger;
        private readonly ITenantService _tenantService;

        public MultiTenantEfUow(TContext c, ITenantService tenantService, ILogger<MultiTenantEfUow<TEntity, TContext>> logger)
        {
            _c = c;
            _tenantService = tenantService;
            _logger = logger;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _c.ChangeTracker.Entries<TEntity>().Select(ee => ee.Entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var tenant = await _tenantService.GetCurrentTenantAsync();
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            SetTenantId(tenant.TenantId);
            await _c.SaveChangesAsync(cancellationToken);
            
            stopWatch.Stop();
            _logger.LogDebug("MultiTenantEfUow.SaveChangesAsync for {EntityType} took {ElapsedMilliseconds} ms", typeof(TEntity).Name, stopWatch.ElapsedMilliseconds);
        }

        private void SetTenantId(Guid tenantId)
        {
            var multiTenantEntities =
                _c.ChangeTracker.Entries()
                    .Where(e => e.IsMultiTenant() && e.State != EntityState.Unchanged);

            foreach (var e in multiTenantEntities)
            {
                var attemptedTenantId = e.GetTenantId();
                if (attemptedTenantId.HasValue && attemptedTenantId != tenantId)
                {
                    throw new Exception(
                        $"Attempted to save entities for TenantId {attemptedTenantId} in the context of TenantId {tenantId}");
                }
                e.SetTenantId(tenantId);
            }
        }

    }
}