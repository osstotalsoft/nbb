using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusMediator
    {
        Task<TResponse> Send<TResponse>(IQuery request, CancellationToken cancellationToken = default(CancellationToken))
            where TResponse : class;           
    }
}