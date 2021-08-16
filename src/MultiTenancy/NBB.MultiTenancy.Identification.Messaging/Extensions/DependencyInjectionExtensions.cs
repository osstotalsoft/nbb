// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Messaging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDefaultMessagingTenantIdentification(this IServiceCollection services)
        {
            
            services.AddTenantIdentificationService()
                .AddTenantIdentificationStrategy<IdTenantIdentifier>(builder => builder
                    .AddTenantTokenResolver<TenantIdHeaderMessagingTokenResolver>(MessagingHeaders.TenantId));

            return services;
        }
    }
}
