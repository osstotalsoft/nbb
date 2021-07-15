using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class TryWith
    {
        public class SideEffect<T> : ISideEffect<T>, IAmHandledBy<Handler<T>>
        {
            public Effect<T> InnerEffect { get; }
            public Func<Exception, Effect<T>> ExceptionHandler { get; }

            public SideEffect(Effect<T> innerEffect, Func<Exception, Effect<T>> exceptionHandler)
            {
                InnerEffect = innerEffect;
                ExceptionHandler = exceptionHandler;
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
                try {
                    return await _interpreter.Interpret(sideEffect.InnerEffect, cancellationToken);
                }
                catch (Exception ex){
                    var effect = sideEffect.ExceptionHandler.Invoke(ex);
                    return await _interpreter.Interpret(effect, cancellationToken);
                }
            }
        }

        public static SideEffect<T> From<T>(Effect<T> innerEffect, Func<Exception, Effect<T>> exceptionHandler)
        {
            return new(innerEffect, exceptionHandler);
        }
    }
}
