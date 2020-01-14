using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.MultiTenancy.Identification
{
    public class TenantIdentificationPair
    {
        public TenantIdentificationPair(IEnumerable<ITenantTokenResolver> tenantTokenResolvers, ITenantIdentifier tenantIdentifier)
        {
            var tokenResolvers = tenantTokenResolvers.ToList();
            TenantTokenResolvers = tokenResolvers.Any() ? tokenResolvers : throw new ArgumentException(nameof(tenantTokenResolvers));
            TenantIdentifier = tenantIdentifier ?? throw new ArgumentNullException(nameof(tenantIdentifier));
        }

        public IEnumerable<ITenantTokenResolver> TenantTokenResolvers { get; }
        public ITenantIdentifier TenantIdentifier { get; }
    }
}
