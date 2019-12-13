using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using System.Collections.Generic;
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
        private readonly MultitenantDbContextHelper _multitenantDbContextHelper;
        private readonly ITenantService _tenantService;

        public MultiTenantEfUow(TContext c, MultitenantDbContextHelper multitenantDbContextHelper, ITenantService tenantService)
        {
            _c = c;
            _multitenantDbContextHelper = multitenantDbContextHelper;
            _tenantService = tenantService;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _c.ChangeTracker.Entries<TEntity>().Select(ee => ee.Entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantService.GetCurrentTenantAsync();
            _multitenantDbContextHelper.CheckContextIntegrity(_c, tenant);
            await _c.SaveChangesAsync(cancellationToken);            
        }
    }
}