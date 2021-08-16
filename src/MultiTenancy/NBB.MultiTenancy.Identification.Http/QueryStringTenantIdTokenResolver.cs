// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class QueryStringTenantIdTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _parameterName;

        public QueryStringTenantIdTokenResolver(IHttpContextAccessor httpContextAccessor, string parameterName)
        {
            _httpContextAccessor = httpContextAccessor;
            _parameterName = parameterName;
        }

        public Task<string> GetTenantToken()
        {
            var parameterValues = _httpContextAccessor?.HttpContext?.Request?.Query[_parameterName];
            if (!parameterValues.HasValue || parameterValues.Value == StringValues.Empty || parameterValues.Value.Count != 1)
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult(parameterValues.Value.ToString());
        }
    }
}
