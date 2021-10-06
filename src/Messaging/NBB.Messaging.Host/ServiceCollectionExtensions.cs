// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Internal;

namespace NBB.Messaging.Host
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingHost(this IServiceCollection serviceCollection,
            Action<IMessagingHostBuilder> configure)
        {
            serviceCollection.AddSingleton<IMessagingHost, MessagingHost>();
            serviceCollection.AddHostedService<MessagingHostService>();
            serviceCollection.AddSingleton<MessagingContextAccessor>();
            serviceCollection.Decorate<IMessageBusPublisher, MessagingContextBusPublisherDecorator>();
            serviceCollection.AddSingleton<ITransportMonitorHandler, HostTransportMonitor>();


            serviceCollection.TryAddSingleton(serviceCollection);

            var builder = new MessagingHostBuilder(serviceCollection);
            configure.Invoke(builder);

            return serviceCollection;
        }

        public class HostTransportMonitor : ITransportMonitorHandler
        {
            private readonly IMessagingHost messagingHost;
            private readonly ILogger<HostTransportMonitor> logger;

            public HostTransportMonitor(IMessagingHost messagingHost, ILogger<HostTransportMonitor> logger)
            {
                this.messagingHost = messagingHost;
                this.logger = logger;
            }
            public void OnError(Exception error)
            {
                var delay = TimeSpan.FromSeconds(10);

                logger.LogInformation($"Restarting host in {delay}");

                messagingHost.ScheduleRestart(delay);
            }
        }
    }
}
