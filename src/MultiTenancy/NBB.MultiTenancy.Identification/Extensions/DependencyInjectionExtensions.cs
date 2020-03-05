﻿using Microsoft.Extensions.DependencyInjection;


namespace NBB.MultiTenancy.Identification.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static TenantServiceBuilder AddTenantService(this IServiceCollection services)
        {
            return new TenantServiceBuilder(services);
        }
    }
}
