// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.OpenTelemetry.Subscriber;
using OpenTelemetry.Trace;
using System;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.OpenTelemetry.Publisher;

namespace NBB.Messaging.OpenTelemetry
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddMessageBusInstrumentation(this TracerProviderBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder), "Must not be null");
            }

            builder
                .AddSource(MessagingActivitySource.Current.Name)
                .ConfigureServices(services =>
                {
                    services.Decorate<IMessageBusPublisher, OpenTelemetryPublisherDecorator>();
                    services.Decorate<IMessageBusSubscriber, OpenTelemetrySubscriberDecorator>();
                });

            return builder;
        }
    }
}
