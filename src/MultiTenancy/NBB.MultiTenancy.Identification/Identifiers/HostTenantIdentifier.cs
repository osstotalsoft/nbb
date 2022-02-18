// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Abstractions.Repositories;
using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Identifiers
{
    public class HostTenantIdentifier : ITenantIdentifier
    {
        private readonly ITenantRepository _tenantRepository;

        public HostTenantIdentifier(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<Guid> GetTenantIdAsync(string tenantToken)
        {
            var tenant = await _tenantRepository.GetByHost(tenantToken);
            return tenant.TenantId;
        }
    }
}
