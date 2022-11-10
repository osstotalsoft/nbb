// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using Serilog.Core;
using Serilog.Events;

namespace NBB.Tools.Serilog.Enrichers.TenantId
{
    public class TenantEnricher : ILogEventEnricher
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;

        public const string TenantIdPropertyName = "TenantId";
        public const string TenantCodePropertyName = "TenantCode";

        public TenantEnricher(ITenantContextAccessor tenantContextAccessor, IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _tenancyOptions = tenancyOptions;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_tenancyOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                return;
            }

            //var tenantId = _tenantContextAccessor.TenantContext?.TryGetTenantId();
            //logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(PropertyName, tenantId));

            var tenant = _tenantContextAccessor.TenantContext?.Tenant;
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(TenantIdPropertyName, tenant?.TenantId));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(TenantCodePropertyName, tenant?.Code ?? "NONE"));
        }
    }
}
