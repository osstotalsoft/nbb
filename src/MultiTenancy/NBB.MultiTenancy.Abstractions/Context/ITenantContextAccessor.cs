namespace NBB.MultiTenancy.Abstractions.Context
{
    public interface ITenantContextAccessor
    {
        TenantContext TenantContext { get; set; }
        TenantContextFlow ChangeTenantContext(TenantContext context);
    }
}
