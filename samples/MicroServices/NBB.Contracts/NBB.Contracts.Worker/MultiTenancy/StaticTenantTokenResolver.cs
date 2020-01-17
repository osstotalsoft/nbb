using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NBB.MultiTenancy.Identification.Resolvers;

namespace NBB.Contracts.Worker.MultiTenancy
{
    public class StaticTenantTokenResolver : ITenantTokenResolver
    {
        private const string TenantIdKey = "TenantId";
        private readonly string _tenantId;

        public StaticTenantTokenResolver(IConfiguration configuration)
        {
            _tenantId = configuration[TenantIdKey];
        }

        public Task<string> GetTenantToken()
        {
            if (string.IsNullOrEmpty(_tenantId))
            {
                throw new CannotResolveTokenException();
            }

            return Task.FromResult(_tenantId);
        }
    }
}
