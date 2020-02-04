using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.MultiTenancy.Abstractions.Hosting
{
    public class TenancyHostingValidator : IHostedService
    {
        private readonly ITenantHostingConfigService _tenantHostingConfigService;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;
        private readonly ILogger<TenancyHostingValidator> _logger;
        private readonly IApplicationLifetime _applicationLifetime;

        public TenancyHostingValidator(ITenantHostingConfigService tenantHostingConfigService, IOptions<TenancyHostingOptions> tenancyOptions, ILogger<TenancyHostingValidator> logger, IApplicationLifetime applicationLifetime)
        {
            _tenantHostingConfigService = tenantHostingConfigService;
            _tenancyOptions = tenancyOptions;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        private void CheckMonoTenant()
        {
            if (_tenancyOptions.Value.TenancyType != TenancyType.MonoTenant) return;
            var tenantId = _tenancyOptions.Value.TenantId ?? throw new ApplicationException("MonoTenant Id is not configured");

            if (_tenantHostingConfigService.IsShared(tenantId))
            {
                _logger.LogCritical($"Attempting to start host for shared tenant {tenantId} in a MonoTenant context");
                _applicationLifetime.StopApplication();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CheckMonoTenant();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
