using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface ISideEffectHandler<in TSideEffect, TOutput>
        where TSideEffect : ISideEffect<TOutput>
    {
        Task<TOutput> Handle(TSideEffect sideEffect, CancellationToken cancellationToken = default);
    }
}
