using System;
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
                .AddClasses(classes => classes.AssignableTo(typeof(IProjector<,,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());


            var metadata = ScanProjectors(assemblies);
            foreach (var m in metadata)
            {
                var serviceType = typeof(INotificationHandler<>).MakeGenericType(m.EventType);
                var implementationType =
                    typeof(EventProjector<,,>).MakeGenericType(m.EventType, m.ProjectionType, m.IdentityType);
                services.AddScoped(serviceType, implementationType);
            }
            
            foreach (var (projectionType, identityType) in metadata.Select(m=> (m.ProjectionType, m.IdentityType)).Distinct())
            {
                var serviceType = typeof(IProjectionStore<,>).MakeGenericType(projectionType, identityType);
                var implementationType = typeof(InMemoryProjectionStore<,>).MakeGenericType(projectionType, identityType);
                services.AddScoped(serviceType, implementationType);
            }

            foreach (var oneOfType in metadata.Select(m=>m.EventType).Where(x=> x.IsOneOfType()).Distinct())
            {
                var innerEventTypes = oneOfType.GetSumOfTypes();
                foreach (var innerEventType in innerEventTypes)
                {
                    var serviceType = typeof(INotificationHandler<>).MakeGenericType(innerEventType);
                    var implementationType =
                        typeof(OneOfNotificationHandlerAdapter<,>).MakeGenericType(oneOfType, innerEventType);
                    services.AddScoped(serviceType, implementationType);
                }
            }


            return services;
        }

        private static (Type EventType, Type ProjectionType, Type IdentityType)[] ScanProjectors(
            params Assembly[] assemblies)
            => assemblies
                .SelectMany(a => a.GetTypes())
                .SelectMany(t =>
                    t.GetInterfaces()
                        .Where(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IProjector<,,>))
                        .Select(i => i.GenericTypeArguments)
                        .Select(args => (EventType: args[0], ProjectionType: args[1], IdentityType: args[2])))
                .ToArray();
    }
}
