using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Services
{
    public class TenantService : ITenantService
    {
        private readonly IEnumerable<TenantIdentificationStrategy> _tenantIdentificationStrategies;

        public TenantService(IEnumerable<TenantIdentificationStrategy> tenantIdentificationStrategies)
        {
            _tenantIdentificationStrategies = tenantIdentificationStrategies;
        }

        public async Task<Guid> GetTenantIdAsync()
        {
            foreach (var tenantIdentificationStrategy in _tenantIdentificationStrategies)
            {
                var tenantId = await tenantIdentificationStrategy.TryGetTenantIdAsync();

                if (tenantId.HasValue)
                {
                    return tenantId.Value;
                }
            }

            throw new TenantNotFoundException();
        }
    }
}