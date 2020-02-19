using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Linq;

namespace NBB.MultiTenancy.Identification.Extensions
{
    public class TenantServiceBuilder
    {
        private readonly IServiceCollection _serviceCollector;

        public TenantServiceBuilder(IServiceCollection serviceCollector)
        {
            _serviceCollector = serviceCollector;
        }

        public void AddTenantIdentificationStrategy<TTenantIdentifier>(params Type[] resolverTypes)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            AddTenantIdentificationStrategy(ImplementationFactory<TTenantIdentifier>, resolverTypes);
        }

        public void AddTenantIdentificationStrategy(ITenantIdentifier identifier, params Type[] resolverTypes)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            AddTenantIdentificationStrategy(ImplementationFactory(identifier), resolverTypes);
        }

        public void AddTenantIdentificationStrategy(Func<IServiceProvider, ITenantIdentifier> implementationFactory, params Type[] resolverTypes)
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

            _serviceCollector.AddSingleton(sp =>
            {
                var identifier = implementationFactory(sp);
                var resolvers = resolverTypes.Select(resolverType => (ITenantTokenResolver)ActivatorUtilities.CreateInstance(sp, resolverType));

                return new TenantIdentificationStrategy(resolvers, identifier);
            });
        }

        public void AddTenantIdentificationStrategy<TTenantIdentifier>(Action<TenantTokenResolverConfiguration> builder)
            where TTenantIdentifier : class, ITenantIdentifier
        {
            AddTenantIdentificationStrategy(ImplementationFactory<TTenantIdentifier>, builder);
        }

        public void AddTenantIdentificationStrategy(ITenantIdentifier identifier, Action<TenantTokenResolverConfiguration> builder)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            AddTenantIdentificationStrategy(ImplementationFactory(identifier), builder);
        }

        public void AddTenantIdentificationStrategy(Func<IServiceProvider, ITenantIdentifier> implementationFactory, Action<TenantTokenResolverConfiguration> builder)
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

            _serviceCollector.AddSingleton(sp =>
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
