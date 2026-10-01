// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using Mediator;
using NBB.ProcessManager.Mediator;
using NBB.ProcessManager.Runtime;
using NBB.Core.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProcessManagerMediatorDependencyInjectionExtensions
    {
        private const string MediatorRegistrationHint =
            "call services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped) in the project that references Mediator.SourceGenerator";

        /// <summary>
        /// Registers a Mediator notification handler for every event handled by the registered process manager definitions,
        /// and the Mediator effects. Call it after AddProcessManager().
        /// Requires Mediator registered with ServiceLifetime Scoped or Transient (validated when the host starts).
        /// </summary>
        public static IServiceCollection AddProcessManagerMediatorHandlers(this IServiceCollection services)
        {
            if (services.All(d => d.ServiceType != typeof(ProcessExecutionCoordinator)))
                throw new InvalidOperationException("AddProcessManager() must be called before AddProcessManagerMediatorHandlers().");

            foreach (var (definitionType, dataType, eventType) in services.GetProcessManagerEventRegistrations())
            {
                services.AddScoped(typeof(INotificationHandler<>).MakeGenericType(eventType),
                    typeof(ProcessManagerNotificationHandler<,,>).MakeGenericType(definitionType, dataType, eventType));
            }

            services.AddMediatorEffects();
            // the bridges depend on scoped services: a Singleton Mediator would resolve them from the root provider
            services
                .RequireRegistration(typeof(IMediator), "NBB.ProcessManager.Mediator", MediatorRegistrationHint)
                .RequireNonSingletonLifetime(typeof(IMediator), "NBB.ProcessManager.Mediator", MediatorRegistrationHint);

            return services;
        }
    }
}
