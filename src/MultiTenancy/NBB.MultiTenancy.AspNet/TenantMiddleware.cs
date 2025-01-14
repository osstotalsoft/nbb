// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Abstractions.Context;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Services;
using NBB.MultiTenancy.Abstractions;
using Microsoft.Extensions.Logging;

namespace NBB.MultiTenancy.AspNet
{
    public class TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
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
                await next(context);
                return;
            }


            var tenantId = await tenantIdentificationService.GetTenantIdAsync();
            var tenant = await tenantRepository.Get(tenantId, context.RequestAborted);

            tenantContextAccessor.TenantContext = new TenantContext(tenant);

            using (logger.BeginScope(new TenantLogScope(tenantContextAccessor.TenantContext)))
            {
                await next(context);
            }
        }
    }
}
