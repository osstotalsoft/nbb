// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Abstractions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static bool IsMultiTenant(this IConfiguration configuration)
        {
            return nameof(TenancyType.MultiTenant).Equals(configuration["MultiTenancy:TenancyType"], System.StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
