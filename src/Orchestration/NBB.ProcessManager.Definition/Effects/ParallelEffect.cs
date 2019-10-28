using System;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class ParallelEffect : IEffect
    {
        public IEffect Effect1 { get; }
        public IEffect Effect2 { get; }

        public ParallelEffect(IEffect effect1, IEffect effect2)
        {
            Effect1 = effect1;
            Effect2 = effect2;
        }

        public Task<Unit> Accept(IEffectVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}