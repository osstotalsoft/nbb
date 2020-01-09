using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Identification.Identifiers;
using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Services
{
    public class TenantService : ITenantService
    {
        private Guid? _tenantId;
        private readonly ITenantIdentifier _identifier;
        private readonly ITenantTokenResolver _tokenResolver;

        public TenantService(ITenantIdentifier identifier, ITenantTokenResolver tokenResolver)
        {
            _identifier = identifier;
            _tokenResolver = tokenResolver;
        }

        public async Task<Guid> GetTenantIdAsync()
        {
            _tenantId ??= await _identifier.GetTenantIdAsync(await _tokenResolver.GetTenantToken());
            if (!_tenantId.HasValue)
            {
                throw new TenantNotFoundException();
            }

            return _tenantId.Value;
        }
    }
}