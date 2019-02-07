using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Data.EntityFramework.Internal;

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

        public Task AddAsync(TEntity entity)
        {
            return _c.Set<TEntity>().AddAsync(entity);
        }

        public async Task Update(TEntity entity)
        {
            var pks = _c.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Select(a => a.Name).ToList();
            var entityPkValues = pks.Select(pk => entity.GetType().GetProperty(pk).GetValue(entity)).ToArray();
            var existingEntity = await _c.Set<TEntity>().FindAsync(entityPkValues);
            
            _c.Entry(existingEntity).CurrentValues.SetValues(entity);
        }

        public async Task Remove(object id)
        {
            object[] ids = (id is object[] list) ? list : new object[] {id};

            var existingEntity = await _c.Set<TEntity>().FindAsync(ids);
            _c.Remove(existingEntity);
        }

        IUow<TEntity> IUowRepository<TEntity>.Uow => _uow;
    }
}
