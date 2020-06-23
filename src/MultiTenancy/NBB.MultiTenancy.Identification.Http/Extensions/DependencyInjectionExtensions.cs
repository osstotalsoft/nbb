﻿using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Extensions;


namespace NBB.MultiTenancy.Identification.Http.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static readonly string DefaultTenantHttpHeaderName = "TenantId";
        public static readonly string DefaultTenantQueryStringParamName = "tenantId";

        public static IServiceCollection AddDefaultHttpTenantIdentification(this IServiceCollection services)
        {
            
            services.AddTenantIdentificationService()
                .AddTenantIdentificationStrategy<IdTenantIdentifier>(builder => builder
                    .AddTenantTokenResolver<TenantIdHeaderHttpTokenResolver>(DefaultTenantHttpHeaderName)
                    .AddTenantTokenResolver<QueryStringTenantIdTokenResolver>(DefaultTenantQueryStringParamName)
                );

            return services;
        }
    }
}
