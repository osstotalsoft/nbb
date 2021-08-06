using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Services;
using NBB.MultiTenancy.Abstractions;

namespace NBB.Messaging.MultiTenancy
{
    /// <summary>
    /// A pipeline middleware that checks if the tenant received in messages is the same as the tenant 
    /// obtained from the current identification strategy and builds the tenant context.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingEnvelope}" />
    public class TenantMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ITenantIdentificationService _tenantIdentificationService;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;
        private readonly ITenantRepository _tenantRepository;

        public TenantMiddleware(ITenantContextAccessor tenantContextAccessor, ITenantIdentificationService tenantIdentificationService, IOptions<TenancyHostingOptions> tenancyOptions, ITenantRepository tenantRepository)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _tenantIdentificationService = tenantIdentificationService;
            _tenancyOptions = tenancyOptions;
            _tenantRepository = tenantRepository;
        }

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            if (_tenantContextAccessor.TenantContext != null)
            {
                throw new ApplicationException("Tenant context is already set");
            }

            if (_tenancyOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                 _tenantContextAccessor.TenantContext = new TenantContext(Tenant.Default);
                await next();
                return;
            }

            var tenantId = await _tenantIdentificationService.GetTenantIdAsync();
            var tenant = await _tenantRepository.Get(tenantId, cancellationToken)
                         ?? throw new ApplicationException($"Tenant {tenantId} not found");

             _tenantContextAccessor.TenantContext = new TenantContext(tenant);

            await next();
        }
    }


    public static partial class MessagingPipelineExtensions
    {
        public static IPipelineBuilder<MessagingContext> UseTenantMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => pipelineBuilder.UseMiddleware<TenantMiddleware, MessagingContext>();
    }
}
