using System.Threading.Tasks;

namespace NBB.Effects.Core
{
    public interface ISideEffectHandler<TSideEffect, TOutput> where TSideEffect : ISideEffect<TOutput>
    {
        Task<TOutput> Handle(TSideEffect sideEffect);
    }
}
