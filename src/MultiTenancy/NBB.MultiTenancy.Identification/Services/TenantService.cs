using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Services
{
    public class TenantService : ITenantService
    {
        private readonly IEnumerable<TenantIdentificationStrategy> _tenantIdentificationPairs;
        private Guid? _tenantId;

        public TenantService(IEnumerable<TenantIdentificationStrategy> tenantIdentificationPairs)
        {
            _tenantIdentificationPairs = tenantIdentificationPairs;
        }

        public async Task<Guid> GetTenantIdAsync()
        {
            if (_tenantId.HasValue)
            {
                return _tenantId.Value;
            }

            foreach (var tenantIdentificationPair in _tenantIdentificationPairs)
            {
                _tenantId = await TryGetTenantIdAsync(tenantIdentificationPair);

                if (_tenantId.HasValue)
                {
                    return _tenantId.Value;
                }
            }

            throw new TenantNotFoundException();
        }

        private static async Task<Guid?> TryGetTenantIdAsync(TenantIdentificationStrategy tenantIdentificationPair)
        {
            var tenantTokenResolvers = tenantIdentificationPair.TenantTokenResolvers;
            var identifier = tenantIdentificationPair.TenantIdentifier;

            foreach (var tokenResolver in tenantTokenResolvers)
            {
                try
                {
                    var tenantToken = await tokenResolver.GetTenantToken();
                    return await identifier.GetTenantIdAsync(tenantToken);
                }
                catch (CannotResolveTokenException) { }
            }

            return null;
        }
    }
}