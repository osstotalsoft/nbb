using System;
using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Resolvers;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http
{
    public class HostRefererHttpTokenResolver : ITenantTokenResolver
    {
        private readonly IHttpContextAccessor _accessor;
        private const string HeaderReferer = "Referer";

        public HostRefererHttpTokenResolver(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Task<string> GetTenantToken()
        {
            var headers = _accessor?.HttpContext?.Request?.Headers;
            if (headers == null || !headers.TryGetValue(HeaderReferer, out var headerReferer))
            {
                return Task.FromResult<string>(null);
            }

            var host = new Uri(headerReferer.ToString()).DnsSafeHost;
            return Task.FromResult(host);

        }
    }
}
