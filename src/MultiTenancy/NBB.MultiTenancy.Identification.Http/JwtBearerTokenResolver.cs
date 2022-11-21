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
    public class JwtBearerTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _parameterName;

        public JwtBearerTokenResolver(IHttpContextAccessor httpContextAccessor, string parameterName)
        {
            _httpContextAccessor = httpContextAccessor;
            _parameterName = parameterName;
        }

        public Task<string> GetTenantToken()
        {

            var hasAuthorizationHeader = _httpContextAccessor?.HttpContext?.Request?.Headers?.ContainsKey(HeaderNames.Authorization) ?? false;
            if (!hasAuthorizationHeader)
            {
                return Task.FromResult((string)null);
            }
            var tokenString = _httpContextAccessor
                .HttpContext
                .Request
                .Headers[HeaderNames.Authorization]
                .ToString()
                .Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(tokenString))
            {
                return Task.FromResult((string)null);
            }
            var jsonToken = handler.ReadToken(tokenString);
            if (jsonToken is not JwtSecurityToken token)
            {
                return Task.FromResult((string)null);
            }
            var claim = token.Claims.FirstOrDefault(x => x.Type == _parameterName);
            return Task.FromResult(claim?.Value);
        }
    }
}
