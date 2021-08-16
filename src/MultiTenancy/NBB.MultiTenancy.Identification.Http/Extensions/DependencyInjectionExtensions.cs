// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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
