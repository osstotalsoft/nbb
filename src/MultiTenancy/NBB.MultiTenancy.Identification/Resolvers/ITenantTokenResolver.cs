using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Resolvers
{
    public interface ITenantTokenResolver
    {
        Task<string> GetTenantToken();
    }
}
