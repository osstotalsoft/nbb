using System;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
 
    public class SequentialEffect : IEffect<Unit>
    {
        public IEffect<Unit> Effect1 { get; }
        public IEffect<Unit> Effect2 { get; }

        public SequentialEffect(IEffect<Unit> effect1, IEffect<Unit> effect2)
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