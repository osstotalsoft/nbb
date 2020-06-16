using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.AspNet
{
     public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITenantService tenantIdentificationService, ITenantContextAccessor tenantContextAccessor)
        {
            if (tenantContextAccessor.TenantContext == null)
            {
                var tenantId = await tenantIdentificationService.GetTenantIdAsync();
                tenantContextAccessor.TenantContext = new TenantContext(new TenantInfo(tenantId, null));
            }

            if (_next != null)
            {
                await _next(context);
            }
        }
    }
}
