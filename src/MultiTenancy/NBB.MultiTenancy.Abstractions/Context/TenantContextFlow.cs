using System;

namespace NBB.MultiTenancy.Abstractions.Context
{
    public readonly struct TenantContextFlow : IDisposable
    {
        private readonly TenantContext _prevContext;
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public TenantContextFlow(ITenantContextAccessor tenantContextAccessor, TenantContext prevContext)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _prevContext = prevContext;
        }

        public void Dispose()
        {
            _tenantContextAccessor.TenantContext = _prevContext;
        }
    }
}
