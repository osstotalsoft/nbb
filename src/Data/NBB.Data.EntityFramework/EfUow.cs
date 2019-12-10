using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework
{
    public class EfUow<TEntity, TContext> : IUow<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly TContext _c;
        private readonly ILogger<EfUow<TEntity, TContext>> _logger;

        public EfUow(TContext c, ILogger<EfUow<TEntity, TContext>> logger)
        {
            _c = c;
            _logger = logger;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _c.ChangeTracker.Entries<TEntity>().Select(ee => ee.Entity);
        }

        public IEnumerable<object> GetAllChanges()
        {
            return _c.ChangeTracker.Entries<TEntity>().Select(ee => ee.Entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await _c.SaveChangesAsync(cancellationToken);

            stopWatch.Stop();
            _logger.LogDebug("EfUow.SaveChangesAsync for {EntityType} took {ElapsedMilliseconds} ms", typeof(TEntity).Name, stopWatch.ElapsedMilliseconds);
        }
    }
}
