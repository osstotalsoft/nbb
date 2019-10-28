using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class PublishMessageEffect : IEffect
    {
        public object Message { get; }

        public PublishMessageEffect(object message)
        {
            Message = message;
        }

        public Task<Unit> Accept(IEffectVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}