using System;

namespace NBB.ProcessManager.Definition.Effects
{
    public class BoundedEffect<TEffectResult1, TEffectResult2> : IEffect<TEffectResult2>
    {
        private readonly Func<TEffectResult1, IEffect<TEffectResult2>> _continuation;

        public BoundedEffect(IEffect<TEffectResult1> effect, Func<TEffectResult1, IEffect<TEffectResult2>> continuation)
        {
            _continuation = continuation;
        }
    }
}