using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class SideEffectHandlerWrapper<TOutput> : ISideEffectHandler<ISideEffect<TOutput>, TOutput>
    {
        private readonly ISideEffectHandler _innerSideEffectHandler;

        public SideEffectHandlerWrapper(ISideEffectHandler innerSideEffectHandler)
        {
            _innerSideEffectHandler = innerSideEffectHandler;
        }

        public async Task<TOutput> Handle(ISideEffect<TOutput> sideEffect,
            CancellationToken cancellationToken = default)
        {
            var task = (_innerSideEffectHandler as dynamic).Handle(sideEffect, cancellationToken);
            if (typeof(TOutput) == typeof(Unit))
            {
                await task;
                return Unit.Value as dynamic;
            }

            return task;
        }
    }
}
