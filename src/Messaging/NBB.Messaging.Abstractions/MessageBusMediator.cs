using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public class MessageBusMediator : IMessageBusMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBusPublisher _messageBusPublisher;

        public MessageBusMediator(IServiceProvider serviceProvider, IMessageBusPublisher messageBusPublisher)
        {
            _serviceProvider = serviceProvider;
            _messageBusPublisher = messageBusPublisher;
        }

        public async Task<TResponse> Send<TResponse>(IQuery request, CancellationToken cancellationToken = default(CancellationToken))
            where TResponse : class
        {
            var messageBusSubscriber =
                _serviceProvider.GetService(typeof(IMessageBusSubscriber<MessagingResponse<TResponse>>)) as
                    IMessageBusSubscriber<MessagingResponse<TResponse>>;

            await _messageBusPublisher.PublishAsync(request, cancellationToken);
            
            var resultEnvelope = await messageBusSubscriber.WaitForMessage(resp => resp.Payload.RequestId == request.QueryId, cancellationToken);
            return resultEnvelope.Payload?.Response;
        }
    }
}
