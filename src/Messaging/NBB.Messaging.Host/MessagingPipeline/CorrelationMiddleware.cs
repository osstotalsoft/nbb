using NBB.Core.Pipeline;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.MessagingPipeline
{
    /// <summary>
    /// Pipeline middleware used to create a correlation scope from the correlation id received in the message headers.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingContext}" />
    public class CorrelationMiddleware : IPipelineMiddleware<MessagingContext>
    {
        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            using (CorrelationManager.NewCorrelationId(context.MessagingEnvelope.GetCorrelationId()))
            {
                await next();
            }
        }
    }
}
