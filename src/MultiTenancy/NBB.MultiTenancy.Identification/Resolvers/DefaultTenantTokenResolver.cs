using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Resolvers
{
    public class DefaultTenantTokenResolver : ITenantTokenResolver
    {
        private readonly Guid _defaultTenantId;

        public DefaultTenantTokenResolver(Guid defaultTenantId)
        {
            _defaultTenantId = defaultTenantId;
        }

        public Task<string> GetTenantToken()
        {
            return Task.FromResult(_defaultTenantId.ToString());
        }
    }
}
