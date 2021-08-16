// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.MultiTenancy.Abstractions.Context
{
    public interface ITenantContextAccessor
    {
        TenantContext TenantContext { get; set; }
        TenantContextFlow ChangeTenantContext(TenantContext context);
    }
}
