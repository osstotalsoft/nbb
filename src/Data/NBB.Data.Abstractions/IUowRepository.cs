using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.Data.Abstractions
{
    public interface IUowRepository<out TEntity>
        where TEntity : class
    {
        IUow<TEntity> Uow { get; }
    }
    
    public static class UowRepositoryExtensions
    {
        public static Task SaveChangesAsync<TEntity>(this IUowRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            return repository.Uow.SaveChangesAsync(cancellationToken);
        }
    }
}