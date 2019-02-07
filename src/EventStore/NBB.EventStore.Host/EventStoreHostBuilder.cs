using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

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

        public EventStoreHostBuilder UsePipeline(Action<IPipelineBuilder<IEvent>> configurePipeline)
        {
            ServiceCollection.AddScoped(serviceProvider =>
            {
                var pipelineBuilder = new PipelineBuilder<IEvent>(serviceProvider);

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
