// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
        public static IPipelineBuilder<MessagingContext> UseOpenTracing(
            this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => UseMiddleware<OpenTracingMiddleware>(pipelineBuilder);


        private static IPipelineBuilder<MessagingContext> UseMiddleware<TMiddleware>(
            this IPipelineBuilder<MessagingContext> pipelineBuilder)
            where TMiddleware : IPipelineMiddleware<MessagingContext>
            => pipelineBuilder.UseMiddleware<TMiddleware, MessagingContext>();
    }
}