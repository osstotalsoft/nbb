using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Services
{
    public interface ITenantService
    {
        /// <summary>
        /// Gets current tenant id
        /// </summary>
        /// <returns>Tenant Id</returns>
        /// <exception cref="TenantNotFoundException"></exception>
        Task<Guid> GetTenantIdAsync();
    }
}