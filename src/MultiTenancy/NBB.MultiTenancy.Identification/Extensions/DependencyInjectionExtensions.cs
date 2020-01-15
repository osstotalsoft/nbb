using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Linq;

namespace NBB.MultiTenancy.Identification.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddResolversForIdentifier<TTenantIdentifier>(this IServiceCollection service, params Type[] resolvers)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.TryAddSingleton<TTenantIdentifier>();
            AddResolversAndStrategy<TTenantIdentifier>(service, resolvers);
        }

        public static void AddResolversForIdentifier<TTenantIdentifier>(this IServiceCollection service, TTenantIdentifier identifier, params Type[] resolvers)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.TryAddSingleton(identifier);
            AddResolversAndStrategy<TTenantIdentifier>(service, resolvers);
        }

        public static void AddResolversForIdentifier<TTenantIdentifier>(this IServiceCollection service, Func<IServiceProvider, TTenantIdentifier> implementationFactory, params Type[] resolvers)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.TryAddSingleton(implementationFactory);
            AddResolversAndStrategy<TTenantIdentifier>(service, resolvers);
        }

        public static void AddResolversForIdentifier<TTenantIdentifier>(this IServiceCollection service, Action<TenantTokenResolverConfiguration> builder)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.TryAddSingleton<TTenantIdentifier>();
            AddResolversAndStrategy<TTenantIdentifier>(service, builder);
        }

        public static void AddResolversForIdentifier<TTenantIdentifier>(this IServiceCollection service, TTenantIdentifier identifier, Action<TenantTokenResolverConfiguration> builder)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.TryAddSingleton(identifier);
            AddResolversAndStrategy<TTenantIdentifier>(service, builder);
        }

        public static void AddResolversForIdentifier<TTenantIdentifier>(this IServiceCollection service, Func<IServiceProvider, TTenantIdentifier> implementationFactory, Action<TenantTokenResolverConfiguration> builder)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            service.TryAddSingleton(implementationFactory);
            AddResolversAndStrategy<TTenantIdentifier>(service, builder);
        }

        private static void AddResolversAndStrategy<TTenantIdentifier>(IServiceCollection service, params Type[] resolvers)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            if (resolvers == null)
            {
                throw new ArgumentNullException(nameof(resolvers));
            }

            if (!resolvers.Any())
            {
                throw new ArgumentException(nameof(resolvers));
            }

            var tokenResolverType = typeof(ITenantTokenResolver);
            if (resolvers.Any(r => !r.IsClass || !tokenResolverType.IsAssignableFrom(r)))
            {
                throw new ArgumentException(nameof(resolvers));
            }

            foreach (var resolver in resolvers)
            {
                service.AddSingleton(typeof(ITenantTokenResolver), resolver);
            }

            service.AddSingleton(sp =>
            {
                var desiredResolvers = sp.GetServices<ITenantTokenResolver>().Where(tr => resolvers.Contains(tr.GetType())).ToList();
                return new TenantIdentificationStrategy(desiredResolvers, sp.GetService<TTenantIdentifier>());
            });
        }

        private static void AddResolversAndStrategy<TTenantIdentifier>(IServiceCollection service, Action<TenantTokenResolverConfiguration> builder)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var config = new TenantTokenResolverConfiguration(service);
            builder.Invoke(config);

            var resolvers = config.GetResolvers();

            if (!resolvers.Any())
            {
                throw new ArgumentException(nameof(builder));
            }

            service.AddSingleton(sp =>
            {
                var desiredResolvers = sp.GetServices<ITenantTokenResolver>().Where(tr => resolvers.Contains(tr.GetType())).ToList();
                return new TenantIdentificationStrategy(desiredResolvers, sp.GetService<TTenantIdentifier>());
            });
        }
    }
}
