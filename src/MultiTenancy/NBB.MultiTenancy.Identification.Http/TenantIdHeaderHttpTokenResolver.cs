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
            _httpContext = httpContextAccessor.HttpContext;
        }

        public Task<string> GetTenantToken()
        {
            var headerValues = _httpContext.Request.Headers[_headerKey];
            if (headerValues == StringValues.Empty || headerValues.Count != 1)
            {
                throw new CannotResolveTokenException();
            }

            return Task.FromResult(headerValues.ToString());
        }
    }
}
