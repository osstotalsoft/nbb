using System.Threading.Tasks;

namespace NBB.Effects.Core
{
    public interface IEffectRunner
    {
        Task<T> RunEffect<T>(IEffect<T> effect);
    }
}
