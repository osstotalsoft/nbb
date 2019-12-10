using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Services
{
    public interface ITenantService
    {
        Task<Tenant> GetCurrentTenantAsync();
    }
}