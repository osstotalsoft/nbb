using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NBB.ProcessManager.Definition;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void AddProcessManagerDefinition(this IServiceCollection services, params Assembly[] assemblies)
        {
            //scan for pm definitions 
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
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

            services.Remove(services.FirstOrDefault(x => x.ImplementationType == processManagerNotificationHandlerImplementationType));

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