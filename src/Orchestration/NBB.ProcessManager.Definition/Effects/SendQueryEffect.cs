using System;

namespace NBB.ProcessManager.Definition.Effects
{
    public class SendQueryEffect<TResult> : IEffect<TResult>
    {
        public object Query { get; }

        public SendQueryEffect(object query)
        {
            Query = query;
        }
    }

    public static class EffectExtensions
    {
        public static IEffect<TEffectResult2> ContinueWith<TEffectResult1, TEffectResult2>(this IEffect<TEffectResult1> effect, 
            Func<TEffectResult1, IEffect<TEffectResult2>> continuation)
        {
            return new BoundedEffect<TEffectResult1, TEffectResult2>(effect, continuation);
        }
    }
}