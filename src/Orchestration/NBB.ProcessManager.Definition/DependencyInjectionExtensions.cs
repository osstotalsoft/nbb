using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.ProcessManager.Definition
{
    public static class DependencyInjectionExtensions
    {
        public static void AddProcessManagerDefinition(this IServiceCollection services)
        {
            //scan for pm definitions 
            services.Scan(scan => scan
                .FromEntryAssembly()
                .AddClasses(classes => classes.AssignableTo(typeof(IDefinition<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );
        }

        public static void AddNotificationHandlers(this IServiceCollection services, Type processManagerNotificationHandlerImplementationType)
        {
            if (!processManagerNotificationHandlerImplementationType.IsGenericType
                || processManagerNotificationHandlerImplementationType.GetGenericTypeDefinition().GetGenericArguments().Length != 3)
                throw new Exception("Invalid definition handler");

            var tempServiceCollection = new ServiceCollection();
            foreach (var serviceDesc in services)
                tempServiceCollection.Add(serviceDesc);

            var sp = tempServiceCollection.BuildServiceProvider();
            var defs = sp.GetRequiredService<IEnumerable<IDefinition>>();

            foreach (var def in defs)
            {
                var dataType = def.GetType().BaseType?.GenericTypeArguments.FirstOrDefault();
                if (dataType == null)
                    throw new Exception("Cannot determine process manager definition data type");

                foreach (var eventType in def.GetEventTypes())
                {
                    services.AddScoped(typeof(INotificationHandler<>).MakeGenericType(eventType),
                        processManagerNotificationHandlerImplementationType.MakeGenericType(def.GetType(), dataType, eventType));
                }
            }
        }
    }
}