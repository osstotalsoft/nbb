using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NBB.MultiTenancy.Identification.Resolvers;

namespace NBB.Contracts.Api.MultiTenancy
{
    public class HttpHeaderTenantTokenResolver : ITenantTokenResolver
    {
        private const string TenantIdHeader = "TenantId";
        private readonly HttpContext _httpContext;

        public HttpHeaderTenantTokenResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public Task<string> GetTenantToken()
        {
            var header = _httpContext.Request.Headers[TenantIdHeader];
            if (header == StringValues.Empty || !header.Any())
            {
                throw new CannotResolveTokenException();
            }

            return Task.FromResult(header.First());
        }
    }
}
