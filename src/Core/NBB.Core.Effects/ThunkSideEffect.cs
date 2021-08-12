// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class Thunk
    {
        public class SideEffect<TResult> : ISideEffect<TResult>, IAmHandledBy<Handler<TResult>>
        {
            public Func<CancellationToken, Task<TResult>> ImpureFn { get; }

            public SideEffect(Func<CancellationToken, Task<TResult>> impureFn)
            {
                ImpureFn = impureFn;
            }


        }

        public class Handler<TResult> : ISideEffectHandler<SideEffect<TResult>, TResult>
        {
            public Task<TResult> Handle(SideEffect<TResult> sideEffect, CancellationToken cancellationToken = default)
            {
                return sideEffect.ImpureFn(cancellationToken);
            }
        }

        public static SideEffect<TResult> From<TResult>(Func<CancellationToken, Task<TResult>> impureFn)
        {
            return new(impureFn);
        }

        public static SideEffect<TResult> From<TResult>(Func<TResult> impureFn)
        {
            return new(ct => Task.FromResult(impureFn()));
        }

        public static SideEffect<Unit> From(Func<CancellationToken, Task> impureFn)
        {
            return new(async ct =>
            {
                await impureFn(ct);
                return Unit.Value;
            });
        }

        public static SideEffect<Unit> From(Action impureFn)
        {
            return new(ct =>
            {
                impureFn();
                return Task.FromResult(Unit.Value);
            });
        }
    }

}
