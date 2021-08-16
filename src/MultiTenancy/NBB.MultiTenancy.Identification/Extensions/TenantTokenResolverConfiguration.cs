// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.MultiTenancy.Identification.Extensions
{
    public class TenantTokenResolverConfiguration
    {
        private readonly List<Func<IServiceProvider, ITenantTokenResolver>> _resolverFactories;

        public TenantTokenResolverConfiguration()
        {
            _resolverFactories = new List<Func<IServiceProvider, ITenantTokenResolver>>();
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver<TTenantTokenResolver>(params object[] args)
            where TTenantTokenResolver : class, ITenantTokenResolver
        {
            return AddTenantTokenResolver(typeof(TTenantTokenResolver), args);
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver(Type resolverType, params object[] args)
        {
            if (resolverType == null)
            {
                throw new ArgumentNullException(nameof(resolverType));
            }

            var tokenResolverType = typeof(ITenantTokenResolver);
            if (!resolverType.IsClass || !tokenResolverType.IsAssignableFrom(resolverType))
            {
                throw new ArgumentException(nameof(resolverType));
            }

            ITenantTokenResolver ImplementationFactory(IServiceProvider serviceProvider) => (ITenantTokenResolver)ActivatorUtilities.CreateInstance(serviceProvider, resolverType, args);

            return AddTenantTokenResolver(ImplementationFactory);
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver(ITenantTokenResolver tenantTokenResolver)
        {
            if (tenantTokenResolver == null)
            {
                throw new ArgumentNullException(nameof(tenantTokenResolver));
            }

            ITenantTokenResolver ImplementationFactory(IServiceProvider _) => tenantTokenResolver;

            return AddTenantTokenResolver(ImplementationFactory);
        }

        public TenantTokenResolverConfiguration AddTenantTokenResolver(Func<IServiceProvider, ITenantTokenResolver> implementationFactory)
        {
            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            _resolverFactories.Add(implementationFactory);

            return this;
        }

        public IEnumerable<ITenantTokenResolver> GetTenantTokenResolvers(IServiceProvider serviceProvider)
        {
            return _resolverFactories.Select(f => f(serviceProvider));
        }
    }
}
