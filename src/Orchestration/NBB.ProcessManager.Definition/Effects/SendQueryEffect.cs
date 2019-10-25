using System;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class SendQueryEffect<TResult> : IEffect<TResult>
    {
        public IRequest<TResult> Query { get; }

        public SendQueryEffect(IRequest<TResult> query)
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