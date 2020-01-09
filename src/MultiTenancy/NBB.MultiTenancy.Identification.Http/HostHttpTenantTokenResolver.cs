using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NBB.MultiTenancy.Identification.Http
{
    public class HostHttpTenantTokenResolver : ITenantTokenResolver
    {
        private readonly HttpContext _httpContext;

        public HostHttpTenantTokenResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public Task<string> GetTenantToken()
        {
            return Task.FromResult(_httpContext.Request.Host.Host);
        }
    }
}
