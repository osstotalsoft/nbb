// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class TenantIdHeaderHttpTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _headerKey;

        public TenantIdHeaderHttpTokenResolver(IHttpContextAccessor httpContextAccessor, string headerKey)
        {
            _httpContextAccessor = httpContextAccessor;
            _headerKey = headerKey;
        }

        public Task<string> GetTenantToken()
        {
            var headerValues = _httpContextAccessor?.HttpContext?.Request?.Headers[_headerKey];
            if (!headerValues.HasValue || headerValues.Value == StringValues.Empty || headerValues.Value.Count != 1)
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult(headerValues.Value.ToString());
        }
    }
}
