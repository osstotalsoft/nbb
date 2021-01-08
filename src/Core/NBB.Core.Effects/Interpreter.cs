using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Core.Effects
{
    internal class IterativeInterpreterVisitor<T> : Effect<T>.IVisitor<(Effect<T>,T)>
    {
        private readonly ISideEffectBroker _sideEffectBroker;

        public IterativeInterpreterVisitor(ISideEffectBroker sideEffectBroker)
        {
            _sideEffectBroker = sideEffectBroker;
        }

        public Task<(Effect<T>, T)> Visit(Effect<T>.Pure eff, CancellationToken cancellationToken)
            => Task.FromResult<(Effect<T>, T)>((null, eff.Value));

        public async Task<(Effect<T>,T)> Visit<TSideEffect, TSideEffectResult>(Effect<T>.Impure<TSideEffect, TSideEffectResult> eff, CancellationToken cancellationToken)
            where TSideEffect : ISideEffect<TSideEffectResult>
        {
            var sideEffectResult = await _sideEffectBroker.Run<TSideEffect, TSideEffectResult>(eff.SideEffect, cancellationToken);
            var nextEffect = eff.Continuations.Apply(sideEffectResult);
            return (nextEffect, default);
        }
    }
    
    public class Interpreter : IInterpreter
    {
        private readonly ISideEffectBroker _sideEffectBroker;

        public Interpreter(ISideEffectBroker sideEffectBroker)
        {
            _sideEffectBroker = sideEffectBroker;
        }

        public async Task<T> Interpret<T>(Effect<T> effect, CancellationToken cancellationToken = default)
        {
            var v = new IterativeInterpreterVisitor<T>(_sideEffectBroker);
            var nextEffect = effect;
            T result;
            do
            {
                (nextEffect, result) = await nextEffect.Accept(v, cancellationToken);
            } while (nextEffect != null);

            return result;
        }

        public static DisposableInterpreter CreateDefault()
            => new();
    }
    
    

    public class DisposableInterpreter : IInterpreter, IDisposable, IAsyncDisposable
    {
        private readonly ServiceProvider _sp;
        private readonly IInterpreter _interpreter;
        public DisposableInterpreter()
        {
            var services = new ServiceCollection();
            services.AddEffects();
            _sp = services.BuildServiceProvider();
            _interpreter = _sp.GetRequiredService<IInterpreter>();
        }

        public Task<T> Interpret<T>(Effect<T> effect, CancellationToken cancellationToken = default)
            => _interpreter.Interpret(effect, cancellationToken);

        public void Dispose()
            => _sp.Dispose();

        public ValueTask DisposeAsync()
            => _sp.DisposeAsync();
    }
}
