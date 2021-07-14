using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class TryFinally
    {
        public class SideEffect<T> : ISideEffect<T>, IAmHandledBy<Handler<T>>
        {
            public Effect<T> InnerEffect { get; }
            public Action Compensation { get; }

            public SideEffect(Effect<T> innerEffect, Action compensation)
            {
                InnerEffect = innerEffect;
                Compensation = compensation;
            }
        }

        public class Handler<T> : ISideEffectHandler<SideEffect<T>, T>
        {
            private readonly IInterpreter _interpreter;

            public Handler(IInterpreter interpreter)
            {
                _interpreter = interpreter;
            }

            public async Task<T> Handle(SideEffect<T> sideEffect, CancellationToken cancellationToken = default)
            {
                try
                {
                    return await _interpreter.Interpret(sideEffect.InnerEffect, cancellationToken);
                }
                finally
                {
                    sideEffect.Compensation.Invoke();
                }
            }
        }

        public static SideEffect<T> From<T>(Effect<T> innerEffect, Action compensation)
        {
            return new(innerEffect, compensation);
        }
    }
}
