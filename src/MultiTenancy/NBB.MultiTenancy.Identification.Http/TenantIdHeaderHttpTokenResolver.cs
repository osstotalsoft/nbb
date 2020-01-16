using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class TenantIdHeaderHttpTokenResolver : ITenantTokenResolver
    {
        private readonly string _headerKey;
        private readonly HttpContext _httpContext;

        public TenantIdHeaderHttpTokenResolver(IHttpContextAccessor httpContextAccessor, string headerKey)
        {
            _headerKey = headerKey;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public Task<string> GetTenantToken()
        {
            return Task.FromResult(_httpContext.Request.Headers[_headerKey].ToString());
        }
    }
}
