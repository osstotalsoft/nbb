using NBB.MultiTenancy.Identification.Repositories;
using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Identifiers
{
    public class HostTenantIdentifier : ITenantIdentifier
    {
        private readonly IHostTenantRepository _hostTenantRepository;

        public HostTenantIdentifier(IHostTenantRepository hostTenantRepository)
        {
            _hostTenantRepository = hostTenantRepository;
        }

        public Task<Guid> GetTenantIdAsync(string tenantToken)
        {
            return _hostTenantRepository.GetTenantId(tenantToken);
        }
    }
}
