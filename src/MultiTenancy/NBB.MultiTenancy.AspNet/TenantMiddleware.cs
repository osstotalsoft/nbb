using System;
using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Abstractions.Context;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Services;

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
            ITenantContextAccessor tenantContextAccessor, ITenantRepository tenantRepository,  IOptions<TenancyHostingOptions> tenancyOptions)
        {
            if (tenancyOptions.Value.TenancyType == TenancyType.None)
            {
                await _next(context);
                return;
            }

            if (tenantContextAccessor.TenantContext != null)
            {
                throw new ApplicationException("Tenant context is already set");
            }

            var tenantId = await tenantIdentificationService.GetTenantIdAsync();
            var tenant = await tenantRepository.Get(tenantId, context.RequestAborted)
                         ?? throw new ApplicationException($"Tenant {tenantId} not found");

            if (tenant.IsShared && tenancyOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                throw new ApplicationException(
                    $"Received a message for shared tenant {tenantId} in a MonoTenant hosting");
            }

            if (!tenant.IsShared && tenancyOptions.Value.TenancyType == TenancyType.MultiTenant)
            {
                throw new ApplicationException(
                    $"Received a message for premium tenant {tenantId} in a MultiTenant (shared) context");
            }

            if (tenancyOptions.Value.TenancyType == TenancyType.MonoTenant && tenancyOptions.Value.TenantId != tenantId)
            {
                throw new ApplicationException(
                    $"Invalid tenant ID. Expected {tenancyOptions.Value.TenantId} but received {tenantId}");
            }


            tenantContextAccessor.TenantContext = new TenantContext(tenant);

            await _next(context);
        }
    }
}
