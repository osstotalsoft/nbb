// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
        public static IServiceCollection AddProcessManagerDefinition(this IServiceCollection services, params Assembly[] assemblies)
        {
            //scan for pm definitions 
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IDefinition<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

            return services;
        }

        /// <summary>
        /// Gets the events handled by the process manager definitions registered in the service collection.
        /// Events that start an obsolete process manager are skipped.
        /// </summary>
        public static IEnumerable<ProcessManagerEventRegistration> GetProcessManagerEventRegistrations(this IServiceCollection services)
        {
            var tempServiceCollection = new ServiceCollection();
            foreach (var serviceDesc in services)
                tempServiceCollection.Add(serviceDesc);

            using var sp = tempServiceCollection.BuildServiceProvider();
            var defs = sp.GetRequiredService<IEnumerable<IDefinition>>();
            var registrations = new List<ProcessManagerEventRegistration>();

            foreach (var def in defs)
            {
                var dataType = def.GetType().BaseType?.GenericTypeArguments.FirstOrDefault();
                if (dataType == null)
                    throw new Exception("Cannot determine process manager definition data type");

                foreach (var (eventType, startsProcess) in def.GetEventTypes())
                {
                    if (startsProcess && def.IsObsolete())
                    {
                        continue;
                    }

                    registrations.Add(new ProcessManagerEventRegistration(def.GetType(), dataType, eventType));
                }
            }

            return registrations;
        }
    }
}
