using System;
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

        public Task<TOutput> Handle(ISideEffect<TOutput> sideEffect, CancellationToken cancellationToken = default)
        {
            return (_innerSideEffectHandler as dynamic).Handle(sideEffect, cancellationToken);
        }
    }
}
