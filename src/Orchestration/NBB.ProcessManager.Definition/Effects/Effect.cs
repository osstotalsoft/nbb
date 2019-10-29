using System;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Definition.Effects
{
    public class Effect<TResult> : IEffect<TResult>
    {
        public Func<IEffectRunner, Task<TResult>> Computation { get; }

        public Effect(Func<IEffectRunner, Task<TResult>> computation)
        {
            Computation = computation;
        }
    }
}