// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using MediatR;
using NBB.ProjectR;
using NBB.ProjectR.MediatR;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProjectRMediatRDependencyInjectionExtensions
    {
        /// <summary>
        /// Registers a MediatR notification handler for every event the projectors subscribe to. Call it after AddProjectR().
        /// </summary>
        public static IServiceCollection AddProjectRMediatRHandlers(this IServiceCollection services)
        {
            if (services.All(d => d.ServiceType != typeof(ProjectorMetadataAccessor)))
                throw new InvalidOperationException("AddProjectR() must be called before AddProjectRMediatRHandlers().");

            foreach (var r in services.GetProjectorEventRegistrations())
            {
                services.AddScoped(typeof(INotificationHandler<>).MakeGenericType(r.EventType),
                    typeof(ProjectorNotificationHandler<,,,>).MakeGenericType(r.EventType, r.ModelType, r.MessageType, r.IdentityType));
            }

            return services;
        }
    }
}
