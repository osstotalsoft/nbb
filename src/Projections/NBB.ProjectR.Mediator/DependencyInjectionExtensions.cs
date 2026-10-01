// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using Mediator;
using NBB.ProjectR;
using NBB.ProjectR.Mediator;
using NBB.Core.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProjectRMediatorDependencyInjectionExtensions
    {
        private const string MediatorRegistrationHint =
            "call services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped) in the project that references Mediator.SourceGenerator";

        /// <summary>
        /// Registers a Mediator notification handler for every event the projectors subscribe to. Call it after AddProjectR().
        /// Requires Mediator registered with ServiceLifetime Scoped or Transient (validated when the host starts).
        /// </summary>
        public static IServiceCollection AddProjectRMediatorHandlers(this IServiceCollection services)
        {
            if (services.All(d => d.ServiceType != typeof(ProjectorMetadataAccessor)))
                throw new InvalidOperationException("AddProjectR() must be called before AddProjectRMediatorHandlers().");

            foreach (var r in services.GetProjectorEventRegistrations())
            {
                services.AddScoped(typeof(INotificationHandler<>).MakeGenericType(r.EventType),
                    typeof(ProjectorNotificationHandler<,,,>).MakeGenericType(r.EventType, r.ModelType, r.MessageType, r.IdentityType));
            }

            // the bridges depend on scoped services: a Singleton Mediator would resolve them from the root provider
            services
                .RequireRegistration(typeof(IMediator), "NBB.ProjectR.Mediator", MediatorRegistrationHint)
                .RequireNonSingletonLifetime(typeof(IMediator), "NBB.ProjectR.Mediator", MediatorRegistrationHint);

            return services;
        }
    }
}
