using System.Threading;
using System.Threading.Tasks;

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


    }
}
