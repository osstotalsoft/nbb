using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface ISideEffectHandler<TSideEffect, TOutput> where TSideEffect : ISideEffect<TOutput>
    {
        Task<TOutput> Handle(TSideEffect sideEffect);
    }
}
