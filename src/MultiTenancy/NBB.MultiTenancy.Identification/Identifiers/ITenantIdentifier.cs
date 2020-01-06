using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Identifiers
{
    public interface ITenantIdentifier
    {
        Task<Guid> GetTenantIdAsync(string tenantToken);
    }
}
