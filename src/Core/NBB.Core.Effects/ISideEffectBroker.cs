using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface ISideEffectBroker
    {
        Task<T> Run<T>(ISideEffect<T> sideEffect, CancellationToken cancellationToken = default);
    }
}
