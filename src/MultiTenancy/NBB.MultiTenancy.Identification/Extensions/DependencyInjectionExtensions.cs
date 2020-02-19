using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Identification.Services;


namespace NBB.MultiTenancy.Identification.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static TenantServiceBuilder AddTenantService(this IServiceCollection services)
        {
            services.TryAddSingleton<ITenantService, TenantService>();
            return new TenantServiceBuilder(services);
        }
    }
}
