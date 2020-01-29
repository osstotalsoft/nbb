using System;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.MultiTenancy.Abstractions.Hosting
{
    public class TenancyHostingValidator
    {
        private readonly ITenantMessagingConfigService _tenantMessagingConfigService;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;

        public TenancyHostingValidator(ITenantMessagingConfigService tenantMessagingConfigService, IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _tenantMessagingConfigService = tenantMessagingConfigService;
            _tenancyOptions = tenancyOptions;
        }

        public void Validate()
        {
            CheckMonoTenant();
        }

        private void CheckMonoTenant()
        {
            if (_tenancyOptions.Value.TenancyType != TenancyType.MonoTenant) return;
            var tenantId = _tenancyOptions.Value.MonoTenantId ?? throw new ApplicationException("MonoTenant Id is not configured");

            if (_tenantMessagingConfigService.IsShared(tenantId))
            {
                throw  new ApplicationException($"Starting message host for shared tenant {tenantId} in a MonoTenant context");
            }
        }
    }
}
