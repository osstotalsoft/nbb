using Microsoft.AspNetCore.Http;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Services;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Http.Services
{
    public class HostHttpTenantService : AbstractTenantService
    {
        private readonly HttpContext _httpContext;

        public HostHttpTenantService(ITenantIdentifier identifier, IHttpContextAccessor httpContextAccessor) : base(identifier)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override Task<string> GetTenantToken()
        {
            return Task.FromResult(_httpContext.Request.Host.Host);
        }
    }
}
