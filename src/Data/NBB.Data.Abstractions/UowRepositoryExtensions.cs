using System.Threading.Tasks;

namespace NBB.Data.Abstractions
{
    public static class UowRepositoryExtensions
    {
        public static Task SaveChangesAsync<TEntity>(this IUowRepository<TEntity> repository)
            where TEntity : class
        {
            return repository.Uow.SaveChangesAsync();
        }
    }
}
