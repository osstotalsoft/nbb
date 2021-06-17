using Microsoft.EntityFrameworkCore.Metadata;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class EntityTypeExtensions
    {
        public static bool IsMultiTenant(this IEntityType entityType) 
            => (bool?)entityType.FindAnnotation(MultiTenancy.MultiTenantAnnotation)?.Value ?? false;
    }
}
