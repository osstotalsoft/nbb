using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;

namespace NBB.Contracts.Worker.MultiTenancy
{
    public class TenantRepositoryMock : ITenantRepository
    {
        public TenantRepositoryMock()
        {
        }

        public Task<Tenant> Get(Guid id, CancellationToken token = default)
        {
            var tenant = new Tenant(id, id.ToString(), true);
            return Task.FromResult(tenant);
        }

        public Task<Tenant> GetByHost(string host, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
