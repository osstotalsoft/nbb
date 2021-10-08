// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Internal;

namespace NBB.Messaging.Host
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingHost(this IServiceCollection serviceCollection, IConfiguration configuration,
            Action<IMessagingHostBuilder> configure)
        {
            serviceCollection.Configure<MessagingHostOptions>(configuration.GetSection("Messaging").GetSection("Host"));

            serviceCollection.AddSingleton<IMessagingHost, MessagingHost>();
            serviceCollection.AddHostedService<MessagingHostService>();
            serviceCollection.AddSingleton<MessagingContextAccessor>();
            serviceCollection.Decorate<IMessageBusPublisher, MessagingContextBusPublisherDecorator>();


            serviceCollection.TryAddSingleton(serviceCollection);

            var builder = new MessagingHostBuilder(serviceCollection);
            configure.Invoke(builder);

            return serviceCollection;
        }
    }
}
