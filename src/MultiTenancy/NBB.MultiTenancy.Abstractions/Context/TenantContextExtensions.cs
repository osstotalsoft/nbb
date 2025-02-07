// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.MultiTenancy.Abstractions.Context
{
    public static class TenantContextExtensions
    {
        public static Guid GetTenantId(this TenantContext tenantContext) => tenantContext?.Tenant?.TenantId ?? throw new TenantNotFoundException();
        public static Guid? TryGetTenantId(this TenantContext tenantContext) => tenantContext?.Tenant?.TenantId;
        public static string GetTenantCode(this TenantContext tenantContext) => tenantContext?.Tenant?.Code ?? throw new TenantNotFoundException();

        public static TenantContextFlow ChangeTenantContext(this ITenantContextAccessor tenantContextAccessor, Tenant tenant)
            => tenantContextAccessor.ChangeTenantContext(new TenantContext(tenant));
    }
}
