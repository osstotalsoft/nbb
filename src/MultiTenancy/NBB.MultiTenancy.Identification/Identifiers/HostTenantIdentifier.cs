// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Abstractions.Repositories;
using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Identifiers
{
    public class HostTenantIdentifier : ITenantIdentifier
    {
        private readonly ITenantRepository _hostTenantRepository;

        public HostTenantIdentifier(ITenantRepository hostTenantRepository)
        {
            _hostTenantRepository = hostTenantRepository;
        }

        public async Task<Guid> GetTenantIdAsync(string tenantToken)
        {
            var tenant = await _hostTenantRepository.GetByHost(tenantToken);
            return tenant.TenantId;
        }
    }
}
