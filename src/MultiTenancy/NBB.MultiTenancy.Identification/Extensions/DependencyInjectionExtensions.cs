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
        public static void AddResolverForIdentifier<T>(this IServiceCollection service, params Type[] resolvers)
            where T : ITenantIdentifier
        {
            if (resolvers.Any(r => !(r is ITenantTokenResolver)))
            {
                throw new ArgumentException();
            }

            foreach (var resolver in resolvers)
            {
                service.TryAddSingleton(resolver);
            }

            service.AddSingleton(sp =>
            {
                var desiredResolvers = sp.GetServices<ITenantTokenResolver>().Where(tr => resolvers.Contains(tr.GetType()));
                return new TenantIdentificationPair(desiredResolvers, sp.GetService<T>());
            });
        }
    }
}
