using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;

namespace NBB.Contracts.Api.MultiTenancy
{
    public class TenantRepositoryMock : ITenantRepository
    {
        private readonly IOptions<TenancyHostingOptions> _tenancyHostingOptions;

        public TenantRepositoryMock(IOptions<TenancyHostingOptions> tenancyHostingOptions)
        {
            _tenancyHostingOptions = tenancyHostingOptions;
        }

        public Task<Tenant> Get(Guid id, CancellationToken token = default)
        {
            var isMonoTenant = _tenancyHostingOptions.Value.TenancyType == TenancyType.MonoTenant &&
                           id == _tenancyHostingOptions.Value.TenantId;

            var tenant = new Tenant(id, id.ToString(), !isMonoTenant);
            return Task.FromResult(tenant);
        }

        public Task<Tenant> GetByHost(string host, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
