using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.Core.Effects;

namespace NBB.Application.Effects
{
    public class MediatorSendQuery
    {
        public class SideEffect<TResponse> : ISideEffect<TResponse>, IAmHandledBy<Handler<TResponse>>
        {
            public IQuery<TResponse> Query { get; }

            public SideEffect(IQuery<TResponse> query)
            {
                Query = query;
            }
        }


        public class Handler<TResponse> : ISideEffectHandler<SideEffect<TResponse>, TResponse>
        {
            public Task<TResponse> Handle(SideEffect<TResponse> sideEffect)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
