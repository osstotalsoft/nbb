using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using NBB.MultiTenancy.Identification.Services;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace NBB.MultiTenancy.Identification.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddTenantIdentification(this IServiceCollection services)
        {
            services.TryAddSingleton<ITenantService, TenantService>();
            return services;
        }

        public static void AddTenantIdentificationStrategy<TTenantIdentifier>(this IServiceCollection service, params Type[] resolverTypes)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.AddTenantIdentificationStrategy(ImplementationFactory<TTenantIdentifier>, resolverTypes);
        }

        public static void AddTenantIdentificationStrategy(this IServiceCollection service, ITenantIdentifier identifier, params Type[] resolverTypes)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            service.AddTenantIdentificationStrategy(ImplementationFactory(identifier), resolverTypes);
        }

        public static void AddTenantIdentificationStrategy(this IServiceCollection service, Func<IServiceProvider, ITenantIdentifier> implementationFactory, params Type[] resolverTypes)
        {
            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            if (resolverTypes == null)
            {
                throw new ArgumentNullException(nameof(resolverTypes));
            }

            var resolverInterfaceType = typeof(ITenantTokenResolver);
            if (!resolverTypes.Any() || resolverTypes.Any(rt => !resolverInterfaceType.IsAssignableFrom(rt)))
            {
                throw new ArgumentException(nameof(resolverTypes));
            }

            service.AddSingleton(sp =>
            {
                var identifier = implementationFactory(sp);
                var resolvers = resolverTypes.Select(resolverType => (ITenantTokenResolver)ActivatorUtilities.CreateInstance(sp, resolverType));

                return new TenantIdentificationStrategy(resolvers, identifier);
            });
        }

        public static void AddTenantIdentificationStrategy<TTenantIdentifier>(this IServiceCollection service, Action<TenantTokenResolverConfiguration> builder)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.AddTenantIdentificationStrategy(ImplementationFactory<TTenantIdentifier>, builder);
        }

        public static void AddTenantIdentificationStrategy(this IServiceCollection service, ITenantIdentifier identifier, Action<TenantTokenResolverConfiguration> builder)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            service.AddTenantIdentificationStrategy(ImplementationFactory(identifier), builder);
        }

        public static void AddTenantIdentificationStrategy(this IServiceCollection service, Func<IServiceProvider, ITenantIdentifier> implementationFactory, Action<TenantTokenResolverConfiguration> builder)
        {
            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var config = new TenantTokenResolverConfiguration();
            builder.Invoke(config);

            service.AddSingleton(sp =>
            {
                var identifier = implementationFactory(sp);
                var resolvers = config.GetTenantTokenResolvers(sp);

                return new TenantIdentificationStrategy(resolvers, identifier);
            });
        }

        private static TTenantIdentifier ImplementationFactory<TTenantIdentifier>(IServiceProvider serviceProvider) where TTenantIdentifier : class, ITenantIdentifier
            => ActivatorUtilities.CreateInstance<TTenantIdentifier>(serviceProvider);

        private static Func<IServiceProvider, TTenantIdentifier> ImplementationFactory<TTenantIdentifier>(TTenantIdentifier identifier) where TTenantIdentifier : class, ITenantIdentifier
            => _ => identifier;
    }
}
