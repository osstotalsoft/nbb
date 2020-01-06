using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Threading.Tasks;
using NBB.MultiTenancy.Identification.Identifiers;

namespace NBB.MultiTenancy.Identification.Services
{
    public abstract class AbstractTenantService : ITenantService
    {
        protected ITenantIdentifier Identifier;

        protected AbstractTenantService(ITenantIdentifier identifier)
        {
            Identifier = identifier;
        }

        public virtual async Task<Guid> GetTenantIdAsync()
        {
            var token = await GetTenantToken();
            return await Identifier.GetTenantIdAsync(token);
        }

        public abstract Task<string> GetTenantToken();
    }
}
