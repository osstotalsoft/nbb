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
        private readonly TContext _dbContext;
        private readonly ILogger<MultiTenantEfUow<TEntity, TContext>> _logger;
        private readonly ITenantService _tenantService;

        public MultiTenantEfUow(TContext dbContext, ITenantService tenantService, ILogger<MultiTenantEfUow<TEntity, TContext>> logger)
        {
            _dbContext = dbContext;
            _tenantService = tenantService;
            _logger = logger;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _dbContext.ChangeTracker.Entries<TEntity>().Select(ee => ee.Entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var tenantId = await _tenantService.GetTenantIdAsync();
            if (tenantId.Equals(Guid.Empty))
            {
                throw new Exception("Tenant could not be identified");
            }

            _dbContext.SetTenantId(tenantId);
            await _dbContext.SaveChangesAsync(cancellationToken);

            stopWatch.Stop();
            _logger.LogDebug("MultiTenantEfUow.SaveChangesAsync for {EntityType} took {ElapsedMilliseconds} ms", typeof(TEntity).Name, stopWatch.ElapsedMilliseconds);
        }
    }
}