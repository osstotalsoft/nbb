﻿using System.Reflection;
using MediatR;
using NBB.ProjectR;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProjectR(this IServiceCollection services, params Assembly[] assemblies)
        {
            var metadata = ProjectorMetadataService.ScanProjectorsMetadata(assemblies);
            foreach (var m in metadata)
            {
                services.AddSingleton(typeof(IProjector<,,>).MakeGenericType(m.ModelType, m.MessageType, m.IdentityType), m.ProjectorType);

                foreach (var eventType in m.SubscriptionTypes)
                {
                    var serviceType = typeof(INotificationHandler<>).MakeGenericType(eventType);
                    var implementationType =
                        typeof(ProjectorNotificationHandler<,,,>).MakeGenericType(eventType, m.ModelType, m.MessageType, m.IdentityType);
                    services.AddScoped(serviceType, implementationType);
                }
                
                services.AddScoped(typeof(IReadModelStore<>).MakeGenericType(m.ModelType), typeof(ProjectionStore<,,>).MakeGenericType(m.ModelType, m.MessageType, m.IdentityType));
            }
            
            services.AddScoped(typeof(IProjectionStore<,,>), typeof(ProjectionStore<,,>));

            services.AddSingleton(typeof(ProjectorMetadataAccessor), _ => new ProjectorMetadataAccessor(metadata));

            return services;
        }
    }
}
