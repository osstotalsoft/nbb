using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    
    public class NoEffect : IEffect
    {
        public static readonly IEffect Instance = new NoEffect();

        public Task<Unit> Accept(IEffectVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}