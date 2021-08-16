// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using NBB.Data.EventSourcing.Infrastructure;
using NBB.Domain.Abstractions;
using NBB.Data.Abstractions;
using NBB.Data.EventSourcing;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEventSourcingDataAccess(this IServiceCollection services, Action<IServiceProvider, EventSourcingOptionsBuilder> optionsAction = null)
        {
            services.AddSingleton(
                serviceProvider =>
                {
                    var builder = new EventSourcingOptionsBuilder();
                    optionsAction?.Invoke(serviceProvider, builder);
                    return builder.Options;
                });

            return services;
        }

        public static IServiceCollection AddEventSourcedRepository<TAggregateRoot>(this IServiceCollection services)
            where TAggregateRoot : class, IEventSourcedAggregateRoot, new()
        {
            services.AddScoped<IEventSourcedRepository<TAggregateRoot>, EventSourcedRepository<TAggregateRoot>>();

            return services;
        }
    }
}
