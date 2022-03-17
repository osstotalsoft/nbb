// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Configuration
{
    public partial class TenantInfrastructure
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantConfiguration _tenantConfiguration;
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public TenantInfrastructure(ITenantRepository tenantRepository, ITenantConfiguration tenantConfiguration, ITenantContextAccessor tenantContextAccessor)
        {
            _tenantRepository = tenantRepository;
            _tenantConfiguration = tenantConfiguration;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task<List<string>> GetConnectionStrings(string name)
        {
            _tenantContextAccessor.TenantContext ??= new TenantContext(Tenant.Default);

            var tenants = await _tenantRepository.GetAll();

            var result = tenants.Select(t =>
            {
                _tenantContextAccessor.ChangeTenantContext(t);

                var connectionString = _tenantConfiguration.GetConnectionString(name);
                return connectionString;
            })
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToList();
            return result;
        }
    }
}
