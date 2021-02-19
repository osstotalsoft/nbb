﻿using System;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

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
        public static IPipelineBuilder<MessagingContext> UseMiddleware<TMiddleware>(this IPipelineBuilder<MessagingContext> pipelineBuilder) where TMiddleware : IPipelineMiddleware<MessagingContext>
            => pipelineBuilder.UseMiddleware<TMiddleware, MessagingContext>();

        /// <summary>
        /// Adds to the pipeline a middleware that creates a correlation scope from the correlation id received in the message headers.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingContext> UseCorrelationMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => UseMiddleware<CorrelationMiddleware>(pipelineBuilder);

        /// <summary>
        /// Adds to the pipeline a middleware that logs and swallows all exceptions.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingContext> UseExceptionHandlingMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => UseMiddleware<ExceptionHandlingMiddleware>(pipelineBuilder);

        /// <summary>
        /// Adds to the pipeline a middleware that sends/publishes messages that are events, commands or queries to mediatR.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingContext> UseMediatRMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => UseMiddleware<MediatRMiddleware>(pipelineBuilder);

        /// <summary>
        /// Adds to the pipeline a middleware that offers resiliency  policies for "out of order" and concurrency exceptions.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingContext> UseDefaultResiliencyMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => UseMiddleware<DefaultResiliencyMiddleware>(pipelineBuilder);

        public static IPipelineBuilder<MessagingContext> When(this IPipelineBuilder<MessagingContext> pipeline,
            bool condition, Func<IPipelineBuilder<MessagingContext>, IPipelineBuilder<MessagingContext>> innerBuilder)
            => condition ? innerBuilder.Invoke(pipeline) : pipeline;

        /// <summary>
        /// Adds to the pipeline a middleware used to interpret effects generated by the given message payload handler
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <param name="messageHandler">A handler that receives the message payload and returns an effect.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingContext> UseEffectMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder, Func<object, Effect<Unit>> messageHandler)
            => pipelineBuilder.Use(async (context, token, next) =>
            {
                var interpreter = context.Services.GetRequiredService<IInterpreter>();
                var effect = messageHandler(context.MessagingEnvelope.Payload);

                await interpreter.Interpret(effect, token);
                await next.Invoke();
            });
    }
}
