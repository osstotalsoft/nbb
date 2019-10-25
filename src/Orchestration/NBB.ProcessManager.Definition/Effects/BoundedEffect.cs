using System;

namespace NBB.ProcessManager.Definition.Effects
{
    public class BoundedEffect<TEffectResult1, TEffectResult2> : IEffect<TEffectResult2>
    {
        public IEffect<TEffectResult1> Effect { get; }
        public Func<TEffectResult1, IEffect<TEffectResult2>> Continuation { get; }

        public BoundedEffect(IEffect<TEffectResult1> effect, Func<TEffectResult1, IEffect<TEffectResult2>> continuation)
        {
            Effect = effect;
            Continuation = continuation;
        }
    }
}