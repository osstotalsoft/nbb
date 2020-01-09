using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification
{
    public interface ITenantTokenResolver
    {
        Task<string> GetTenantToken();
    }
}
