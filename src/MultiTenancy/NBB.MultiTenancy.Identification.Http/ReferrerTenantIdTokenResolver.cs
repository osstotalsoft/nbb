// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    /// <summary>
    /// services.AddTenantTokenResolver<TenantIdRefererHttpTokenResolver>("nbb-tenantId=([({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?)")
    /// </summary>
    public class ReferrerTenantIdTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string HeaderNameDefault = "Referer";
        private readonly string _regEx;

        public ReferrerTenantIdTokenResolver(string regEx, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _regEx = regEx;
        }

        public Task<string> GetTenantToken()
        {
            if (_httpContextAccessor?.HttpContext?.Request == null)
            {
                return Task.FromResult((string)null);
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(HeaderNameDefault, out var value))
            {
                var url = value.ToString();
                var match = Regex.Match(url, _regEx, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var tenantId = match.Groups[1].Value;
                    return Task.FromResult(tenantId);
                }
            }

            return Task.FromResult((string)null);
        }
    }
}
