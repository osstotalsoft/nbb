﻿using System;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.ProjectR.ProjectionStores;

namespace NBB.ProjectR
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProjectR(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IProject<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(ICorrelate<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
                


            var metadata =
                assemblies
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t =>
                        t.GetInterfaces()
                            .Where(i =>
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IProject<,>))
                            .Select(i =>
                            {
                                var args = i.GenericTypeArguments;
                                var eventType = args[0];
                                var projectionType = args[1];
                                var identityType = GetIdentityTypeFrom(projectionType);
                                return new {eventType, projectionType, identityType};
                            }))
                    .ToList();
            
            foreach (var m in metadata)
            {
                var serviceType = typeof(INotificationHandler<>).MakeGenericType(m.eventType);
                var implementationType =
                    typeof(EventProjector<,,>).MakeGenericType(m.eventType, m.projectionType, m.identityType);
                services.AddScoped(serviceType, implementationType);
            }
            
            foreach (var projectionType in metadata.Select(m=> m.projectionType).Distinct())
            {
                var identityType = GetIdentityTypeFrom(projectionType);
                var serviceType = typeof(IProjectionStore<,>).MakeGenericType(projectionType, identityType);
                var implementationType = typeof(InMemoryProjectionStore<,>).MakeGenericType(projectionType, identityType);
                services.AddScoped(serviceType, implementationType);
            }


            return services;
        }

        private static Type GetIdentityTypeFrom(Type projectionType)
        {
            var identityMarkerInterface =
                projectionType
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHaveIdentityOf<>));
            
             return identityMarkerInterface?.GetGenericArguments().First();
        }


    }
}
