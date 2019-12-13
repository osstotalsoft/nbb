using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class Extensions
    {
        private static readonly string MULTITENANT_ANNOTATION = "custom:multitenant";

        public static EntityTypeBuilder<TEntity> IsMultiTenant<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class
        {
            builder.HasAnnotation(MULTITENANT_ANNOTATION, true);
            return builder;
        }

        public static bool IsMultiTenant(this IEntityType entity)
        {
            var annotation = entity.FindAnnotation(MULTITENANT_ANNOTATION);
            if (annotation != null)
            {
                return (bool)annotation.Value;
            }

            return false;
        }
    }
}