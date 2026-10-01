// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using MediatR;
using NBB.ProcessManager.Runtime;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProcessManagerMediatRDependencyInjectionExtensions
    {
        /// <summary>
        /// Registers a MediatR notification handler for every event handled by the registered process manager definitions,
        /// and the MediatR effects. Call it after AddProcessManager().
        /// </summary>
        public static IServiceCollection AddProcessManagerMediatRHandlers(this IServiceCollection services)
        {
            if (services.All(d => d.ServiceType != typeof(ProcessExecutionCoordinator)))
                throw new InvalidOperationException("AddProcessManager() must be called before AddProcessManagerMediatRHandlers().");

            // guard against the open generic handler being registered by MediatR assembly scanning
            services.Remove(services.FirstOrDefault(x => x.ImplementationType == typeof(ProcessManagerNotificationHandler<,,>)));

            foreach (var (definitionType, dataType, eventType) in services.GetProcessManagerEventRegistrations())
            {
                services.AddScoped(typeof(INotificationHandler<>).MakeGenericType(eventType),
                    typeof(ProcessManagerNotificationHandler<,,>).MakeGenericType(definitionType, dataType, eventType));
            }

            services.AddMediatREffects();

            return services;
        }
    }
}
