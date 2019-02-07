using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using NBB.Data.EventSourcing.Infrastructure;
using NBB.Domain.Abstractions;
using NBB.Data.Abstractions;

namespace NBB.Data.EventSourcing
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
