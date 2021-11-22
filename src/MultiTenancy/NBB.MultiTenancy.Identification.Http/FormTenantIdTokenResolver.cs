// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class FormTenantIdTokenResolver : ITenantTokenResolver
    {
        private readonly string _key;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FormTenantIdTokenResolver(string key, IHttpContextAccessor httpContextAccessor)
        {
            _key = key;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<string> GetTenantToken()
        {
            if (_httpContextAccessor?.HttpContext?.Request == null)
            {
                return Task.FromResult((string)null);
            }

            if (_httpContextAccessor.HttpContext.Request.HasFormContentType && _httpContextAccessor.HttpContext.Request.Form.ContainsKey(_key))
            {
                var tenantId = _httpContextAccessor.HttpContext.Request.Form[_key];
                return Task.FromResult(tenantId.ToString());
            }
            return Task.FromResult((string)null);
        }
    }
}
