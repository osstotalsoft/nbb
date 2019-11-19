using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.Abstractions
{
    public interface IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : class
    {
        Task SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default);
        Task<TAggregateRoot> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    }
}
