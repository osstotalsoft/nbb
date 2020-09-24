using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class Interpreter : IInterpreter
    {
        private readonly ISideEffectMediator _sideEffectMediator;

        public Interpreter(ISideEffectMediator sideEffectMediator)
        {
            _sideEffectMediator = sideEffectMediator;
        }

        public async Task<T> Interpret<T>(IEffect<T> effect, CancellationToken cancellationToken = default)
        {
            var v = new IterativeInterpreterVisitor<T>(_sideEffectMediator, this);
            var nextEffect = effect;
            T result;
            do
            {
                (nextEffect, result) = await nextEffect.Accept(v, cancellationToken);
            } while (nextEffect != null);

            return result;
        }

       
    }
}
