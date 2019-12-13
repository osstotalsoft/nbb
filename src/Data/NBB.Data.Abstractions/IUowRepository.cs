using NBB.Core.Abstractions;

namespace NBB.Data.Abstractions
{
    public interface IUowRepository<out TEntity>
        where TEntity : class
    {
        IUow<TEntity> Uow { get; }
    }
}