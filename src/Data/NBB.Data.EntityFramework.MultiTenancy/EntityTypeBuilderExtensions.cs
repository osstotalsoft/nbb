using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class EntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder<T> IsMultiTenant<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            builder.HasAnnotation(MultiTenancy.MultiTenantAnnotation, true);

            return builder;
        }


        public static EntityTypeBuilder<T> WithTenantIdColumn<T>(this EntityTypeBuilder<T> builder, string tenantIdComlumnName) where T : class
        {
            builder.HasAnnotation(MultiTenancy.MultiTenantColumnAnnotation, tenantIdComlumnName);

            return builder;
        }

        internal static EntityTypeBuilder<T> AddTenantIdProperty<T>(this EntityTypeBuilder<T> builder, string tenantIdColumn = null) where T : class
        {
            var prop = builder.Property<Guid>(MultiTenancy.TenantIdProp);

            if (string.IsNullOrWhiteSpace(tenantIdColumn))
                return builder;

            prop.HasColumnName(tenantIdColumn);

            return builder;
        }

        internal static EntityTypeBuilder<T> AddTenantIdQueryFilter<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            // https://github.com/dotnet/efcore/pull/11017
            DbContext dummyDbContext = null;
            builder.HasQueryFilter(t => EF.Property<Guid>(t, MultiTenancy.TenantIdProp) == dummyDbContext.GetTenantIdFromContext());

            return builder;
        }
    }
}
