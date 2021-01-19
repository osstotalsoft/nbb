using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.OpenTracing.Subscriber
{
    public static class MessagingPipelineExtensions
    {
        /// <summary>
        /// Adds to the pipeline a middleware that creates an OpenTracing span.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingEnvelope> UseOpenTracing(
            this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => UseMiddleware<OpenTracingMiddleware>(pipelineBuilder);


        private static IPipelineBuilder<MessagingEnvelope> UseMiddleware<TMiddleware>(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder) where TMiddleware : IPipelineMiddleware<MessagingEnvelope>
            => pipelineBuilder.UseMiddleware<TMiddleware, MessagingEnvelope>();
    }
}
