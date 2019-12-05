using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface IInterpreter
    {
        Task<T> Interpret<T>(IEffect<T> effect, CancellationToken cancellationToken = default);
    }

    public static class InterpreterExtensions
    {
        public static async Task Interpret(this IInterpreter interpreter, IEffect effect, CancellationToken cancellationToken = default)
        {
            var _ = await interpreter.Interpret(effect, cancellationToken);
        }
    }
}
