using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;

namespace NBB.MultiTenancy.Data.EntityFramework.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEfMultiTenantServices(this IServiceCollection services)
        {
            services
            .Decorate(typeof(IUow<>), typeof(MultitenantUowDecorator<>));

            return services;
        }
    }
}