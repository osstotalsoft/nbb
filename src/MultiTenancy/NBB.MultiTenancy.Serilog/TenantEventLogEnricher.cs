using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using Serilog.Core;
using Serilog.Events;

namespace NBB.MultiTenancy.Serilog
{
    public class TenantEventLogEnricher : ILogEventEnricher
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;

        public TenantEventLogEnricher(ITenantContextAccessor tenantContextAccessor, IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _tenancyOptions = tenancyOptions;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_tenancyOptions.Value.TenancyType == TenancyType.MonoTenant || _tenantContextAccessor.TenantContext == null)
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(nameof(Tenant.Default.TenantId), Tenant.Default.TenantId));
                return;
            }

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(nameof(Tenant.Default.TenantId), _tenantContextAccessor.TenantContext.GetTenantId()));
        }
    }
}
