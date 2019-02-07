using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Pipeline
{
    public static class PipelineBuilderExtensions
    {

        /// <summary>
        /// Adds the a middleware that is defined inline to the pipeline.
        /// </summary>
        /// <typeparam name="T">The type of data/context processed in the pipeline.</typeparam>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <param name="middleware">The middleware.</param>
        /// <returns>
        /// The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.
        /// </returns>
        public static IPipelineBuilder<T> Use<T>(this IPipelineBuilder<T> pipelineBuilder, Func<T, CancellationToken, Func<Task>, Task> middleware)
        {
            return pipelineBuilder.Use(next =>
            {
                return (data, token) =>
                {
                    Func<Task> simpleNext = () => next(data, token);
                    return middleware(data, token, simpleNext);
                };
            });
        }

        /// <summary>
        /// Adds a middleware of type <typeparamref name="TMiddleware"/> the pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The type of the middleware.</typeparam>
        /// <typeparam name="T">The type of data/context processed in the pipeline.</typeparam>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>
        /// The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.
        /// </returns>
        public static IPipelineBuilder<T> UseMiddleware<TMiddleware, T>(this IPipelineBuilder<T> pipelineBuilder) where TMiddleware : IPipelineMiddleware<T>
        {
            return pipelineBuilder.Use(
                (data, cancellationToken, next) =>
                {
                    var instance = (IPipelineMiddleware<T>)ActivatorUtilities.CreateInstance(pipelineBuilder.ServiceProvider, typeof(TMiddleware));
                    return instance.Invoke(data, cancellationToken, next);
                }
            );
        }
    }
}
