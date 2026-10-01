// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                    var processorType =
                        typeof(ProjectorEventProcessor<,,,>).MakeGenericType(eventType, m.ModelType, m.MessageType, m.IdentityType);
                    services.AddScoped(processorType);
                }
                
                services.AddScoped(typeof(IReadModelStore<>).MakeGenericType(m.ModelType), typeof(ProjectionStore<,,>).MakeGenericType(m.ModelType, m.MessageType, m.IdentityType));
            }
            
            services.AddScoped(typeof(IProjectionStore<,,>), typeof(ProjectionStore<,,>));

            services.AddSingleton(new ProjectorMetadataAccessor(metadata));

            return services;
        }

        /// <summary>
        /// Gets the events the projectors registered with AddProjectR() subscribe to. Used by the mediator adapters.
        /// </summary>
        public static IEnumerable<ProjectorEventRegistration> GetProjectorEventRegistrations(this IServiceCollection services)
            => services
                .Where(d => d.ServiceType == typeof(ProjectorMetadataAccessor) && d.ImplementationInstance is ProjectorMetadataAccessor)
                .SelectMany(d => ((ProjectorMetadataAccessor)d.ImplementationInstance).Metadata)
                .SelectMany(m => m.SubscriptionTypes.Select(eventType => new ProjectorEventRegistration(eventType, m.ModelType, m.MessageType, m.IdentityType)))
                .ToList();
    }
}
