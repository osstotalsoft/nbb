using System;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Contracts.Api.MultiTenancy
{
    public class TenantMessagingConfigService : ITenantMessagingConfigService
    {
        private readonly IOptions<TenancyOptions> _tenancyOptions;
        public TenantMessagingConfigService(IOptions<TenancyOptions> tenancyOptions)
        {
            _tenancyOptions = tenancyOptions;
        }

        public bool IsShared(Guid tenantId) => _tenancyOptions.Value.TenancyContextType == TenancyContextType.MultiTenant;
    }
}
