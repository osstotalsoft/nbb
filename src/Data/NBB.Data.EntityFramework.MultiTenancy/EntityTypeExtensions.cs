// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class EntityTypeExtensions
    {
        public static bool IsMultiTenant(this IEntityType entityType) 
            => (bool?)entityType.FindAnnotation(MultiTenancy.MultiTenantAnnotation)?.Value ?? false;

        public static bool IsMultiTenant(this IMutableEntityType entityType)
            => IsMultiTenant(entityType as IEntityType);
    }
}
