using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Core.Effects
{
    public class Interpreter : IInterpreter
    {
        private readonly ISideEffectBroker _sideEffectBroker;

        public Interpreter(ISideEffectBroker sideEffectBroker)
        {
            _sideEffectBroker = sideEffectBroker;
        }

        public async Task<T> Interpret<T>(Effect<T> effect, CancellationToken cancellationToken = default)
        {
            var nextEffect = effect;
            T result;
            do
            {
                (nextEffect, result) = await nextEffect.Run(_sideEffectBroker, cancellationToken);
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
