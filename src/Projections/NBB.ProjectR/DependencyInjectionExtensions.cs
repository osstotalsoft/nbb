using System;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.ProjectR
{
    public static class DependencyInjectionExtensions
    {
        record ProjectorMetadata(Type ProjectorType, Type ProjectionType, Type[] EventTypes);

        public static IServiceCollection AddProjectR(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IProjector<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());


            var metadata = ScanProjectorsMetadata(assemblies);
            foreach (var (projectorType, projectionType, eventTypes) in metadata)
            {
                services.AddSingleton(typeof(IProjector<>).MakeGenericType(projectionType), projectorType);
                services.AddScoped(typeof(IProjectionStore<>).MakeGenericType(projectionType),
                    typeof(ProjectionStore<>).MakeGenericType(projectionType));
                
                foreach (var eventType in eventTypes)
                {
                    var serviceType = typeof(INotificationHandler<>).MakeGenericType(eventType);
                    var implementationType =
                        typeof(ProjectorNotificationHandler<,>).MakeGenericType(eventType, projectionType);
                    services.AddScoped(serviceType, implementationType);
                }

            }


            return services;
        }

        private static ProjectorMetadata[] ScanProjectorsMetadata(
            params Assembly[] assemblies)
            => assemblies
                .SelectMany(a => a.GetTypes())
                .SelectMany(projectorType =>
                    projectorType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IProjector<>))
                    .Select(i => i.GetGenericArguments().First())
                    .Select(projectionType => new ProjectorMetadata(projectorType, projectionType, projectorType.GetSubscriptionTypes())))
                .ToArray();
    }
}
