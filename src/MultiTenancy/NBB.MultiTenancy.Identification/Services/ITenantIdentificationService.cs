using System;
using System.Threading.Tasks;
using NBB.MultiTenancy.Abstractions;

namespace NBB.MultiTenancy.Identification.Services
{
    public interface ITenantIdentificationService
    {
        /// <summary>
        /// Gets current tenant id
        /// </summary>
        /// <returns>Tenant Id</returns>
        /// <exception cref="TenantNotFoundException"></exception>
        Task<Guid> GetTenantIdAsync();

        /// <summary>
        /// Tries to get current tenant id
        /// </summary>
        /// <returns>Tenant Id or null</returns>
        Task<Guid?> TryGetTenantIdAsync();
    }
}