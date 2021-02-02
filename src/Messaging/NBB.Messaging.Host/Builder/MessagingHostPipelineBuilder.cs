using System;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Host.Builder
{
    /// <summary>
    /// Used to configure the messaging host pipeline
    /// </summary>
    public class MessagingHostPipelineBuilder
    {
        private readonly IServiceCollection _serviceCollection;

        public MessagingHostPipelineBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds the message processing pipeline to the messaging host.
        /// </summary>
        /// <param name="configurePipeline">The pipeline configurator is used to add the middleware to the pipeline.</param>
        /// <returns>The messaging host builder to further configure the messaging host. It is used in the fluent API</returns>
        public void UsePipeline(Action<IPipelineBuilder<MessagingEnvelope>> configurePipeline)
        {
            _serviceCollection.AddScoped(serviceProvider =>
            {
                var pipelineBuilder = new PipelineBuilder<MessagingEnvelope>(serviceProvider);

                configurePipeline(pipelineBuilder);

                return pipelineBuilder.Pipeline;
            });
        }
    }
}
