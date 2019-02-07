using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Resiliency
{
    public static class ServiceCollectionExtensions
    {
        public static void AddResiliency(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IResiliencyPolicyProvider, ResiliencyPolicyProvider>();
        }
    }
}
