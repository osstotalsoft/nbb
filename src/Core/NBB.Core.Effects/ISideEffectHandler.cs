using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{

    public interface ISideEffectHandler
    {
    }

    public interface ISideEffectHandler<in TSideEffect, TOutput> : ISideEffectHandler 
        where TSideEffect : ISideEffect<TOutput>
    {
        Task<TOutput> Handle(TSideEffect sideEffect, CancellationToken cancellationToken = default);
    }

    public interface ISideEffectHandler<in TSideEffect> : ISideEffectHandler
        where TSideEffect : ISideEffect
    {
        Task Handle(TSideEffect sideEffect, CancellationToken cancellationToken = default);
    }
}
