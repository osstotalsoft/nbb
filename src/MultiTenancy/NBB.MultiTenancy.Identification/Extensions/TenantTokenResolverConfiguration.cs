using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Collections.Generic;

namespace NBB.MultiTenancy.Identification.Extensions
{
    public class TenantTokenResolverConfiguration
    {
        private readonly IServiceCollection _service;
        private readonly List<Type> _resolvers;

        public TenantTokenResolverConfiguration(IServiceCollection service)
        {
            _service = service;
            _resolvers = new List<Type>();
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver<TTenantTokenResolver>()
            where TTenantTokenResolver : class, ITenantTokenResolver
        {
            _service.AddSingleton<ITenantTokenResolver, TTenantTokenResolver>();
            _resolvers.Add(typeof(TTenantTokenResolver));

            return this;
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver(ITenantTokenResolver tenantTokenResolverType)
        {
            if (tenantTokenResolverType == null)
            {
                throw new ArgumentNullException(nameof(tenantTokenResolverType));
            }

            _service.AddSingleton(tenantTokenResolverType);
            _resolvers.Add(tenantTokenResolverType.GetType());

            return this;
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver(Type resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            var tokenResolverType = typeof(ITenantTokenResolver);
            if (!resolver.IsClass || !tokenResolverType.IsAssignableFrom(resolver))
            {
                throw new ArgumentException(nameof(resolver));
            }

            _service.AddSingleton(tokenResolverType, resolver);
            _resolvers.Add(resolver);

            return this;
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver<TTenantTokenResolver>(Func<IServiceProvider, TTenantTokenResolver> implementationFactory)
            where TTenantTokenResolver : class, ITenantTokenResolver
        {
            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            _service.AddSingleton<ITenantTokenResolver>(implementationFactory);
            _resolvers.Add(typeof(TTenantTokenResolver));

            return this;
        }

        public Type[] GetResolvers()
        {
            return _resolvers.ToArray();
        }
    }
}
