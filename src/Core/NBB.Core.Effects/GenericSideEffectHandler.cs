// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class GenericSideEffectHandler<TSideEffect, TOutput>: ISideEffectHandler<TSideEffect, TOutput>
        where TSideEffect : ISideEffect<TOutput>
    {
        private readonly Func<TSideEffect, CancellationToken, Task<TOutput>> handlerFn;

        public GenericSideEffectHandler(Func<TSideEffect, CancellationToken, Task<TOutput>> handlerFn)
        {
            this.handlerFn = handlerFn;
        }

        public Task<TOutput> Handle(TSideEffect sideEffect, CancellationToken cancellationToken = default)
        {
            return handlerFn(sideEffect, cancellationToken);
        }
    }
}
