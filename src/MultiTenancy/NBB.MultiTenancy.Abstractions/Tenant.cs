using System;

namespace NBB.MultiTenancy.Abstractions
{
    public class Tenant
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; }
    }
}