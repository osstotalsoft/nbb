// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using NBB.MultiTenancy.Identification.Resolvers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class BearerTokenTenantIdTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _parameterName;

        public BearerTokenTenantIdTokenResolver(IHttpContextAccessor httpContextAccessor, string parameterName)
        {
            _httpContextAccessor = httpContextAccessor;
            _parameterName = parameterName;
        }

        public Task<string> GetTenantToken()
        {
            if (!_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                return Task.FromResult(string.Empty);
            }
            var tokenString = _httpContextAccessor
                .HttpContext
                .Request
                .Headers[HeaderNames.Authorization]
                .ToString()
                .Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(tokenString);
            if (jsonToken is not JwtSecurityToken token)
            {
                return default;
            }
            var claim = token.Claims.FirstOrDefault(x => x.Type == _parameterName);
            return Task.FromResult(claim?.Value);
        }
    }
}
