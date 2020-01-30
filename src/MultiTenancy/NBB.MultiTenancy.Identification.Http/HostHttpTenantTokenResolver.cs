using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class HostHttpTenantTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HostHttpTenantTokenResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<string> GetTenantToken()
        {
            return Task.FromResult(_httpContextAccessor?.HttpContext?.Request?.Host.Host);
        }
    }
}
