using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NBB.MultiTenancy.Abstractions.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHostingConfigValidation(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, TenancyHostingValidator>();
        }
    }
}
