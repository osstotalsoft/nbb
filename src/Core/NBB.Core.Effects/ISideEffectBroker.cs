using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface ISideEffectBroker
    {
        Task<TSideEffectResult> Run<TSideEffect, TSideEffectResult>(TSideEffect sideEffect, CancellationToken cancellationToken = default)
            where TSideEffect : ISideEffect<TSideEffectResult>;
    }
}
