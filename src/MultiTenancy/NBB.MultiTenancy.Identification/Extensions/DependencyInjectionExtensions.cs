using NBB.MultiTenancy.Identification.Extensions;


// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static TenantServiceBuilder AddTenantIdentificationService(this IServiceCollection services)
        {
            return new TenantServiceBuilder(services);
        }
    }
}
