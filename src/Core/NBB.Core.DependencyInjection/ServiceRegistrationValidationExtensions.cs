// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Core.DependencyInjection
{
    /// <summary>
    /// Registration requirements validated when the generic host starts (<c>ValidateOnStart</c>).
    /// They inspect the service collection, so registrations made after the call are taken into account.
    /// </summary>
    public static class ServiceRegistrationValidationExtensions
    {
        /// <summary>
        /// Fails the generic host at start if <paramref name="serviceType"/> is not registered.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="serviceType">The required service type.</param>
        /// <param name="requiredBy">The feature that requires the service, used in the error message.</param>
        /// <param name="registrationHint">How to register the service, used in the error message.</param>
        public static IServiceCollection RequireRegistration(this IServiceCollection services, Type serviceType,
            string requiredBy, string registrationHint)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(serviceType);

            services.AddOptions<ServiceRegistrationValidationOptions>()
                .Validate(_ => Find(services, serviceType) != null,
                    $"{requiredBy} requires {serviceType.FullName} to be registered: {registrationHint}")
                .ValidateOnStart();

            return services;
        }

        /// <summary>
        /// Fails the generic host at start if <paramref name="serviceType"/> is registered with the Singleton lifetime.
        /// A missing registration is not reported: combine with <see cref="RequireRegistration"/> when the service is mandatory.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="serviceType">The service type whose lifetime is checked.</param>
        /// <param name="requiredBy">The feature that requires a Scoped or Transient lifetime, used in the error message.</param>
        /// <param name="registrationHint">How to register the service with a supported lifetime, used in the error message.</param>
        public static IServiceCollection RequireNonSingletonLifetime(this IServiceCollection services, Type serviceType,
            string requiredBy, string registrationHint)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(serviceType);

            services.AddOptions<ServiceRegistrationValidationOptions>()
                .Validate(_ => Find(services, serviceType) is not { Lifetime: ServiceLifetime.Singleton },
                    $"{requiredBy} requires {serviceType.FullName} with the Scoped or Transient lifetime: {registrationHint}")
                .ValidateOnStart();

            return services;
        }

        // the last registration is the one resolved by the container
        private static ServiceDescriptor Find(IServiceCollection services, Type serviceType)
            => services.LastOrDefault(d => d.ServiceType == serviceType);

        private sealed class ServiceRegistrationValidationOptions
        {
        }
    }
}
