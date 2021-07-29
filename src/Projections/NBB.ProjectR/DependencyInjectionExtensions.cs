using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.ProjectR
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProjectR(this IServiceCollection services, params Assembly[] assemblies)
        {
            var metadata = ProjectorMetadataService.ScanProjectorsMetadata(assemblies);
            foreach (var (projectorType, projectionType, eventTypes, snapshotFrequency) in metadata)
            {
                services.AddSingleton(typeof(IProjector<>).MakeGenericType(projectionType), projectorType);

                foreach (var eventType in eventTypes)
                {
                    var serviceType = typeof(INotificationHandler<>).MakeGenericType(eventType);
                    var implementationType =
                        typeof(ProjectorNotificationHandler<,>).MakeGenericType(eventType, projectionType);
                    services.AddScoped(serviceType, implementationType);
                }


            }
            
            services.AddScoped(typeof(IProjectionStore<>), typeof(ProjectionStore<>));

            services.AddSingleton(typeof(ProjectorMetadataAccessor), _ => new ProjectorMetadataAccessor(metadata));

            return services;
        }
    }
}
