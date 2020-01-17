using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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
            _httpContext = httpContextAccessor?.HttpContext;
        }

        public Task<string> GetTenantToken()
        {
            var headerValues = _httpContext?.Request?.Headers[_headerKey];
            if (!headerValues.HasValue || headerValues.Value == StringValues.Empty || headerValues.Value.Count != 1)
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult(headerValues.Value.ToString());
        }
    }
}
