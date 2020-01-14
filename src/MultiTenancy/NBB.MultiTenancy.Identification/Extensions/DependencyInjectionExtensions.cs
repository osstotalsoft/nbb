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
        public static void AddResolverForIdentifier<TTenantIdentifier>(this IServiceCollection service, params Type[] resolvers)
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

            service.TryAddSingleton<TTenantIdentifier>();

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
    }
}
