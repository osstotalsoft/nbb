// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Data.EntityFramework.Internal;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework
{
    public class EfCrudRepository<TEntity, TContext> : EfReadOnlyRepository<TEntity, TContext>, ICrudRepository<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly TContext _c;
        private readonly IUow<TEntity> _uow;

        public EfCrudRepository(TContext c, IExpressionBuilder expressionBuilder, IUow<TEntity> uow, ILogger<EfCrudRepository<TEntity, TContext>> logger)
            :base(c, expressionBuilder, logger)
        {
            _c = c;
            _uow = uow;
            c.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _c.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        public async Task Update(TEntity entity, CancellationToken cancellationToken = default)
        {
            var pks = _c.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Select(a => a.Name).ToList();
            var entityPkValues = pks.Select(pk => entity.GetType().GetProperty(pk).GetValue(entity)).ToArray();
            var existingEntity = await _c.Set<TEntity>().FindAsync(entityPkValues, cancellationToken);
            
            _c.Entry(existingEntity).CurrentValues.SetValues(entity);
        }

        public async Task Remove(object id, CancellationToken cancellationToken = default)
        {
            object[] ids = (id is object[] list) ? list : new object[] {id};

            var existingEntity = await _c.Set<TEntity>().FindAsync(ids, cancellationToken);
            _c.Remove(existingEntity);
        }

        IUow<TEntity> IUowRepository<TEntity>.Uow => _uow;
    }
}
