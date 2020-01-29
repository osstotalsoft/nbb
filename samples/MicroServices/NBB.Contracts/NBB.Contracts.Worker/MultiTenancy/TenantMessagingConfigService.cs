using System;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Contracts.Worker.MultiTenancy
{
    public class TenantMessagingConfigService : ITenantMessagingConfigService
    {
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;
        public TenantMessagingConfigService(IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _tenancyOptions = tenancyOptions;
        }

        public bool IsShared(Guid tenantId) => _tenancyOptions.Value.TenancyType == TenancyType.MultiTenant;
    }
}
