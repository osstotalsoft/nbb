using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Repositories
{
    public interface IHostTenantRepository
    {
        /// <summary>
        /// Returns a tenant id based on a host information
        /// </summary>
        /// <param name="host">host of the tenant</param>
        /// <returns>tenant id</returns>
        Task<Guid> GetTenantId(string host);
    }
}
