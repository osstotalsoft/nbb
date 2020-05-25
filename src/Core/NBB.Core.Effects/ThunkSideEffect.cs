using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class Thunk
    {
        public class SideEffect<TOutput> : ISideEffect<TOutput>, IAmHandledBy<Handler<TOutput>>
        {
            public Func<CancellationToken, Task<TOutput>> ImpureFn { get; }

            public SideEffect(Func<CancellationToken, Task<TOutput>> impureFn)
            {
                ImpureFn = impureFn;
            }


        }

        public class Handler<TOutput> : ISideEffectHandler<SideEffect<TOutput>, TOutput>
        {
            public Task<TOutput> Handle(SideEffect<TOutput> sideEffect, CancellationToken cancellationToken = default)
            {
                return sideEffect.ImpureFn(cancellationToken);
            }
        }

        public static SideEffect<TOutput> From<TOutput>(Func<CancellationToken, Task<TOutput>> impureFn)
        {
            return new SideEffect<TOutput>(impureFn);
        }

        public static SideEffect<TOutput> From<TOutput>(Func<TOutput> impureFn)
        {
            return new SideEffect<TOutput>(ct => Task.FromResult(impureFn()));
        }

        public static SideEffect<Unit> From(Func<CancellationToken, Task> impureFn)
        {
            return new SideEffect<Unit>(async ct =>
            {
                await impureFn(ct);
                return Unit.Value;
            });
        }

        public static SideEffect<Unit> From(Action impureFn)
        {
            return new SideEffect<Unit>(ct =>
            {
                impureFn();
                return Task.FromResult(Unit.Value);
            });
        }
    }

}
