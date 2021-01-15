using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using System;

namespace NBB.EventStore.Host
{
    public class EventStoreHostBuilder
    {
        public IServiceCollection ServiceCollection { get; }

        public EventStoreHostBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
            AddSubscriberService();
        }

        public EventStoreHostBuilder UsePipeline(Action<IPipelineBuilder<object>> configurePipeline)
        {
            ServiceCollection.AddScoped(serviceProvider =>
            {
                var pipelineBuilder = new PipelineBuilder<object>(serviceProvider);

                configurePipeline(pipelineBuilder);

                return pipelineBuilder.Pipeline;
            });

            return this;
        }

        private EventStoreHostBuilder AddSubscriberService()
        {
            ServiceCollection.AddSingleton<IHostedService, EventStoreSubscriberService>();

            return this;
        }
    }
}
