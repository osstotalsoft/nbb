// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Core.DependencyInjection
{
    public static class SingleImplementationExtensions
    {
        /// <summary>
        /// Registers <typeparamref name="TImplementation"/> as the single implementation of <typeparamref name="TService"/>.
        /// Registering the same implementation again is a no-op; registering it when a different implementation of
        /// <typeparamref name="TService"/> is already registered throws.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="lifetime">The lifetime of the registration.</param>
        /// <param name="conflictHint">Optional text appended to the conflict error message, e.g. how to fix the configuration.</param>
        /// <exception cref="InvalidOperationException">If a different implementation of <typeparamref name="TService"/> is already registered.</exception>
        public static IServiceCollection AddSingleImplementation<TService, TImplementation>(this IServiceCollection services,
            ServiceLifetime lifetime, string conflictHint = null)
            where TService : class
            where TImplementation : class, TService
        {
            ArgumentNullException.ThrowIfNull(services);

            var existing = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
            if (existing != null)
            {
                var existingType = existing.ImplementationType ?? existing.ImplementationInstance?.GetType();
                if (existingType == typeof(TImplementation))
                    return services;

                var message = $"Cannot register {typeof(TImplementation).FullName} as {typeof(TService).FullName}: " +
                              $"{existingType?.FullName ?? "another implementation"} is already registered.";
                throw new InvalidOperationException(string.IsNullOrEmpty(conflictHint) ? message : $"{message} {conflictHint}");
            }

            services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
            return services;
        }
    }
}
