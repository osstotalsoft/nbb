using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NBB.MultiTenancy.Identification.Identifiers;

namespace NBB.MultiTenancy.Identification.Services
{
    public abstract class AbstractTenantService : ITenantService
    {
        private readonly ConcurrentDictionary<string, Guid> _tokenCache;
        protected ITenantIdentifier Identifier;

        protected AbstractTenantService(ITenantIdentifier identifier)
        {
            _tokenCache = new ConcurrentDictionary<string, Guid>();
            Identifier = identifier;
        }

        public virtual async Task<Guid> GetTenantIdAsync()
        {
            var token = await GetTenantToken();

            var tokenId = _tokenCache.GetOrAdd(token, t => Identifier.GetTenantIdAsync(t).Result);

            return tokenId;
        }

        protected abstract Task<string> GetTenantToken();
    }
}