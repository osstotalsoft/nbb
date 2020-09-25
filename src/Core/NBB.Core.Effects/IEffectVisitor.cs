using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface IEffectVisitor<T, TResult>
    {
        Task<TResult> Visit(PureEffect<T> eff, CancellationToken cancellationToken);
        Task<TResult> Visit<T1>(FreeEffect<T1, T> eff, CancellationToken cancellationToken);
        Task<TResult> Visit<T1, T2>(ParallelEffect<T1, T2, T> eff, CancellationToken cancellationToken);
    }
}
