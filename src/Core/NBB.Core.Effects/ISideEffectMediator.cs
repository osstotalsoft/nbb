using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface ISideEffectMediator
    {
        Task<T> Run<T>(ISideEffect<T> sideEffect, CancellationToken cancellationToken = default);
    }
}
