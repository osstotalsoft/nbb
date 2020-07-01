using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;

namespace NBB.MultiTenancy.Abstractions.Hosting
{
    public class TenancyHostingValidator : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;
        private readonly ILogger<TenancyHostingValidator> _logger;
        private readonly IApplicationLifetime _applicationLifetime;

        public TenancyHostingValidator(IServiceScopeFactory scopeFactory,
            IOptions<TenancyHostingOptions> tenancyOptions, ILogger<TenancyHostingValidator> logger,
            IApplicationLifetime applicationLifetime)
        {
            _scopeFactory = scopeFactory;
            _tenancyOptions = tenancyOptions;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        private async Task CheckMonoTenant(CancellationToken cancellationToken)
        {
            if (_tenancyOptions.Value.TenancyType != TenancyType.MonoTenant)
                return;

            var tenantId = _tenancyOptions.Value.TenantId ??
                           throw new ApplicationException("MonoTenant Id is not configured");

            using var scope = _scopeFactory.CreateScope();
            var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
            var tenant = await tenantRepository.Get(tenantId, cancellationToken);

            if (tenant == null)
            {
                _logger.LogCritical($"Tenant {tenantId} was not found in the repository");
                _applicationLifetime.StopApplication();
                return;
            }

            if (tenant.IsShared)
            {
                _logger.LogCritical($"Attempting to start host for shared tenant {tenantId} in a MonoTenant context");
                _applicationLifetime.StopApplication();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await CheckMonoTenant(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
