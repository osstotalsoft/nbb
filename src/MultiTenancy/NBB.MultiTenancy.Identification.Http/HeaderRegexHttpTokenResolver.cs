// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    /// <summary>
    /// services.AddTenantTokenResolver<ReferrerHttpTokenResolver>("nbb-tenantId=([({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?)")
    /// </summary>
    public class HeaderRegexHttpTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _headerName;
        private readonly string _regEx;

        public HeaderRegexHttpTokenResolver(string regEx, string headerName, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _regEx = regEx;
            _headerName = headerName;
        }

        public Task<string> GetTenantToken()
        {
            if (_httpContextAccessor?.HttpContext?.Request == null)
            {
                return Task.FromResult((string)null);
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(_headerName, out var value))
            {
                var headerValue = value.ToString();
                var match = Regex.Match(headerValue, _regEx, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var token = match.Groups[1].Value;
                    return Task.FromResult(token);
                }
            }

            return Task.FromResult((string)null);
        }
    }
}
