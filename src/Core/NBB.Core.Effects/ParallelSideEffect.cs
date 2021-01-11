using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class Parallel
    {
        public class SideEffect<T1, T2> : ISideEffect<(T1, T2)>, IAmHandledBy<Handler<T1, T2>>
        {
            public Effect<T1> LeftEffect { get; }
            public Effect<T2> RightEffect { get; }

            public SideEffect(Effect<T1> leftEffect, Effect<T2> rightEffect)
            {
                LeftEffect = leftEffect;
                RightEffect = rightEffect;
            }
        }

        public class Handler<T1, T2> : ISideEffectHandler<SideEffect<T1, T2>, (T1, T2)>
        {
            private readonly IInterpreter _interpreter;
            
            public Handler(IInterpreter interpreter)
            {
                _interpreter = interpreter;
            }
            
            public async Task<(T1, T2)> Handle(SideEffect<T1, T2> sideEffect, CancellationToken cancellationToken = default)
            {
                var t1 = _interpreter.Interpret(sideEffect.LeftEffect, cancellationToken);
                var t2 = _interpreter.Interpret(sideEffect.RightEffect, cancellationToken);
                await Task.WhenAll(t1, t2);
                return (t1.Result, t2.Result);
            }
        }

        public static SideEffect<T1, T2> From<T1, T2>(Effect<T1> eff1, Effect<T2> eff2)
        {
            return new(eff1, eff2);
        }
    }

}
