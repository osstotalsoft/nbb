// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using Serilog.Core;
using Serilog.Events;

namespace NBB.Tools.Serilog.Enrichers.TenantId
{
    public class TenantEnricher : ILogEventEnricher
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        public static string PropertyName { get; } = nameof(Tenant.Default.TenantId);

        public TenantEnricher(ITenantContextAccessor tenantContextAccessor)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_tenantContextAccessor.TenantContext == null)
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(PropertyName, Tenant.Default.TenantId));
                return;
            }
            var tenantId = _tenantContextAccessor.TenantContext.TryGetTenantId();
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(PropertyName, tenantId ?? Tenant.Default.TenantId));
        }
    }
}
