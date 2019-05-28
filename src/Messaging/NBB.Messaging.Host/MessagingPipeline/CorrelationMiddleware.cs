using NBB.Core.Pipeline;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.MessagingPipeline
{
    /// <summary>
    /// Pipeline middleware used to create a correlation scope from the correlation id received in the message headers.
    /// </summary>
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{NBB.Messaging.DataContracts.MessagingEnvelope}" />
    public class CorrelationMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            using (CorrelationManager.NewCorrelationId(message.GetCorrelationId()))
            {
                await next();
            }
        }
    }
}
