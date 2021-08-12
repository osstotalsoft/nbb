// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using NBB.Messaging.MultiTenancy;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMultiTenantMessaging(this IServiceCollection services)
        {
            services.Decorate<IMessageBusPublisher, MultiTenancyMessageBusPublisherDecorator>();

            return services;
        }
    }
}
