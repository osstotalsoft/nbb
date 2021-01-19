using System.Threading;
using System.Threading.Tasks;
using NBB.ProcessManager.Definition;

namespace NBB.ProcessManager.Runtime.Persistence
{
    public interface IInstanceDataRepository
    {
        Task Save<TData>(Instance<TData> instance, CancellationToken cancellationToken = default) where TData: new();

        Task<Instance<TData>> Get<TData>(IDefinition<TData> definition, object identity, CancellationToken cancellationToken = default) where TData: new();
    }
}