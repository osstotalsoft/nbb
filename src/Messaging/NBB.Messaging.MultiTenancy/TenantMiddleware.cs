// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Linq;

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
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(ITenantContextAccessor tenantContextAccessor, ITenantIdentificationService tenantIdentificationService, IOptions<TenancyHostingOptions> tenancyOptions, ITenantRepository tenantRepository, ILogger<TenantMiddleware> logger)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _tenantIdentificationService = tenantIdentificationService;
            _tenancyOptions = tenancyOptions;
            _tenantRepository = tenantRepository;
            _logger = logger;
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

            Tenant tenant;

            if (context.MessagingEnvelope.Payload is INotification)
            {
                tenant = await TryLoadTenant(context.TopicName, cancellationToken);
                if (tenant == null)
                {
                    return;
                }
            }
            else
            {
                tenant = await LoadTenant(cancellationToken);
            }


            _tenantContextAccessor.TenantContext = new TenantContext(tenant);

            Activity.Current?.SetTag(TracingTags.TenantId, tenant.TenantId);

            await next();
        }

        private async Task<Tenant> LoadTenant(CancellationToken cancellationToken)
        {
            var tenantId = await _tenantIdentificationService.GetTenantIdAsync();
            var tenant = await _tenantRepository.Get(tenantId, cancellationToken)
                            ?? throw new ApplicationException($"Tenant {tenantId} not found");

            return tenant;
        }


        private async Task<Tenant> TryLoadTenant(string topic, CancellationToken cancellationToken)
        {
            var tenantId = await _tenantIdentificationService.TryGetTenantIdAsync();
            if (!tenantId.HasValue)
            {
                _logger.LogDebug("Tenant could not be identified. Message {Topic} will be ignored.", topic);
                return null;
            }

            var tenant = await _tenantRepository.TryGet(tenantId.Value, cancellationToken);
            if (tenant == null)
            {
                _logger.LogDebug("Tenant {Tenant} not found or not enabled. Message {Topic} will be ignored.", tenantId.Value, topic);
                return null;
            }

            return tenant;
        }

    }


    public static partial class MessagingPipelineExtensions
    {
        public static IPipelineBuilder<MessagingContext> UseTenantMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => pipelineBuilder.UseMiddleware<TenantMiddleware, MessagingContext>();
    }
}
