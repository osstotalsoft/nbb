using System;
using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Abstractions.Context;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Services;
using NBB.MultiTenancy.Abstractions;

namespace NBB.MultiTenancy.AspNet
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITenantIdentificationService tenantIdentificationService,
            ITenantContextAccessor tenantContextAccessor, ITenantRepository tenantRepository, IOptions<TenancyHostingOptions> tenancyOptions)
        {
            if (tenantContextAccessor.TenantContext != null)
            {
                throw new ApplicationException("Tenant context is already set");
            }


            if (tenancyOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                tenantContextAccessor.TenantContext = new TenantContext(Tenant.Default);
                await _next(context);
                return;
            }


            var tenantId = await tenantIdentificationService.GetTenantIdAsync();
            var tenant = await tenantRepository.Get(tenantId, context.RequestAborted)
                         ?? throw new ApplicationException($"Tenant {tenantId} not found");

            tenantContextAccessor.TenantContext = new TenantContext(tenant);

            await _next(context);
        }


    }
}
