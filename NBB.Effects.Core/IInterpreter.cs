using System.Threading.Tasks;

namespace NBB.Effects.Core
{
    public interface IInterpreter
    {
        Task<T> Interpret<T>(IEffect<T> effect);
    }
}
