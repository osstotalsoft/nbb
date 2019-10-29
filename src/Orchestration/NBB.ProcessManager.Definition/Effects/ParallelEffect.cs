using System;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class ParallelEffect<TResult>: IEffect<TResult[]>
    {
        public IEffect<TResult>[] Effects { get; }

        public ParallelEffect(params IEffect<TResult>[] effects)
        {
            Effects = effects;
        }

        public Task<TResult[]> Accept(IEffectVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}