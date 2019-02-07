using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Host.MessagingPipeline
{
    public static class MessagingPipelineExtensions
    {
        /// <summary>
        /// Adds the a middleware of type <typeparamref name="TMiddleware"/> to the message processing pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The type of the middleware.</typeparam>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingEnvelope> UseMiddleware<TMiddleware>(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder) where TMiddleware : IPipelineMiddleware<MessagingEnvelope>
            => pipelineBuilder.UseMiddleware<TMiddleware, MessagingEnvelope>();

        /// <summary>
        /// Adds to the pipeline a middleware that creates a correlation scope from the correlation id received in the message headers.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingEnvelope> UseCorrelationMiddleware(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => UseMiddleware<CorrelationMiddleware>(pipelineBuilder);

        /// <summary>
        /// Adds to the pipeline a middleware that logs and swallows all exceptions.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingEnvelope> UseExceptionHandlingMiddleware(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => UseMiddleware<ExceptionHandlingMiddleware>(pipelineBuilder);

        /// <summary>
        /// Adds to the pipeline a middleware that sends/publishes messages that are events, commands or queries to mediatR.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingEnvelope> UseMediatRMiddleware(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => UseMiddleware<MediatRMiddleware>(pipelineBuilder);

        /// <summary>
        /// Adds to the pipeline a middleware that offers resiliency  policies for "out of order" and concurrency exceptions.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingEnvelope> UseDefaultResiliencyMiddleware(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => UseMiddleware<DefaultResiliencyMiddleware>(pipelineBuilder);
    }
}
