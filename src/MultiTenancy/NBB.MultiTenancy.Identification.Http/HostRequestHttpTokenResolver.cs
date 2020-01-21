﻿using System;
using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class HostRequestHttpTokenResolver : ITenantTokenResolver
    {
        private const string HeaderReferer = "Referer";
        private readonly HttpContext _httpContext;

        public HostRequestHttpTokenResolver(IHttpContextAccessor accessor)
        {
            _httpContext = accessor.HttpContext;
        }

        public Task<string> GetTenantToken()
        {
            var headers = _httpContext?.Request?.Headers;
            if (headers == null || !headers.TryGetValue(HeaderReferer, out var headerReferer))
            {
                return Task.FromResult<string>(null);
            }

            var host = new Uri(headerReferer.ToString()).DnsSafeHost;
            return Task.FromResult(host);

        }
    }
}
