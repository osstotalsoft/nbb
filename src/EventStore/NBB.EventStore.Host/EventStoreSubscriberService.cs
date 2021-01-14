﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.EventStore.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Host
{
    public class EventStoreSubscriberService : BackgroundService
    {
        private readonly IEventStoreSubscriber _eventStoreSubscriber;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventStoreSubscriberService> _logger;

        public EventStoreSubscriberService(IEventStoreSubscriber eventStoreSubscriber, IServiceProvider serviceProvider,
            ILogger<EventStoreSubscriberService> logger)
        {
            _eventStoreSubscriber = eventStoreSubscriber;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EventStoreSubscriberService is starting");

            await _eventStoreSubscriber.SubscribeToAllAsync(@event => Handle(@event, cancellationToken), cancellationToken);

            _logger.LogInformation("EventStoreSubscriberService is stopping");
        }


        private async Task Handle(object @event, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pipeline = scope.ServiceProvider.GetService<PipelineDelegate<object>>();

                await pipeline(@event, cancellationToken);
            }
        }
    }
}
