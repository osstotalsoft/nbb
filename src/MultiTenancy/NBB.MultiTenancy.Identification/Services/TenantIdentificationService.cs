using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Services
{
    public class TenantIdentificationService : ITenantIdentificationService
    {
        private readonly IEnumerable<TenantIdentificationStrategy> _tenantIdentificationStrategies;

        public TenantIdentificationService(IEnumerable<TenantIdentificationStrategy> tenantIdentificationStrategies)
        {
            _tenantIdentificationStrategies = tenantIdentificationStrategies;
        }

        public async Task<Guid> GetTenantIdAsync()
        {
            return await TryGetTenantIdAsync() ?? throw new TenantNotFoundException();
        }

        public async Task<Guid?> TryGetTenantIdAsync()
        {
            foreach (var tenantIdentificationStrategy in _tenantIdentificationStrategies)
            {
                var tenantId = await tenantIdentificationStrategy.TryGetTenantIdAsync();

                if (tenantId.HasValue)
                {
                    return tenantId;
                }
            }

            return null;
        }
    }
}