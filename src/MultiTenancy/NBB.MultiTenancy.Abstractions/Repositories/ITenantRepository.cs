using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant> Get(Guid id);
        Task<Tenant> GetByHost(string host);
    }
}
