using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.ProjectR
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProjectR(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IProjector<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());


            var metadata = ProjectorMetadataService.ScanProjectorsMetadata(assemblies);
            foreach (var (projectorType, projectionType, eventTypes, snapshotFrequency) in metadata)
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

            services.AddSingleton(typeof(ProjectorMetadataAccessor), _ => new ProjectorMetadataAccessor(metadata));
            services.AddScoped(typeof(ProjectMessage.Handler<>));

            return services;
        }
    }
}
