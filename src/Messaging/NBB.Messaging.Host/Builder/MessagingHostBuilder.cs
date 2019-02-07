using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Pipeline;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Host.Builder.TypeSelector;
using System;

namespace NBB.Messaging.Host.Builder
{
    public class MessagingHostBuilder
    {
        public IServiceCollection ServiceCollection { get; }

        public MessagingHostBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds the subscriber services to the messaging host.
        /// The subscriber services are background services (hosted services) that consume messages from the bus.
        /// </summary>
        /// <param name="builder">The builder is used to configure the message types or topics for which subscriber services are added.</param>
        /// <returns>The options builder</returns>
        public MessagingHostOptionsBuilder AddSubscriberServices(Action<ITypeSourceSelector> builder)
        {
            var subscriberServiceSelector = new TypeSourceSelector(ServiceCollection);
            builder?.Invoke(subscriberServiceSelector);

            var optionsBuilder =
                new MessagingHostOptionsBuilder(this, ServiceCollection, subscriberServiceSelector, subscriberServiceSelector);

            return optionsBuilder;
        }

        /// <summary>
        /// Adds the message processing pipeline to the messaging host.
        /// </summary>
        /// <param name="configurePipeline">The pipeline configurator is used to add the middleware to the pipeline.</param>
        /// <returns>The messaging host builder to further configure the messaging host. It is used in the fluent API</returns>
        public MessagingHostBuilder UsePipeline(Action<IPipelineBuilder<MessagingEnvelope>> configurePipeline)
        {
            ServiceCollection.AddScoped(serviceProvider =>
            {
                var pipelineBuilder = new PipelineBuilder<MessagingEnvelope>(serviceProvider);

                configurePipeline(pipelineBuilder);

                return pipelineBuilder.Pipeline;
            });

            return this;
        }
    }
}
