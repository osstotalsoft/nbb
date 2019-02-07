using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Mediator.OpenFaaS.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddOpenFaaSMediator(this IServiceCollection services)
        {
            services.AddScoped<IMediator, OpenFaaSMediator>();
        }
    }
}
