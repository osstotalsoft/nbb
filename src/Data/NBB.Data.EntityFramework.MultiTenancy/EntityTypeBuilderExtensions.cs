using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class EntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder<T> IsMultiTenant<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            if (builder.Metadata.IsMultiTenant())
                return builder;

            builder.HasAnnotation(MultiTenancy.MultiTenantAnnotation, true);

            builder.AddTenantIdProperty();
            builder.AddTenantIdQueryFilter();

            return builder;
        }


        public static EntityTypeBuilder<T> WithTenantIdColumn<T>(this EntityTypeBuilder<T> builder, string tenantIdComlumnName) where T : class
        {
            if (builder.Metadata.FindAnnotation(MultiTenancy.MultiTenantAnnotation) != null)
                throw new Exception($"Call '{ nameof(IsMultiTenant) }()' before using '{ nameof(WithTenantIdColumn) }'");

            var prop = builder.Property(MultiTenancy.TenantIdProp).HasColumnName(tenantIdComlumnName);

            return builder;
        }

        private static EntityTypeBuilder<T> AddTenantIdProperty<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            builder.Property<Guid>(MultiTenancy.TenantIdProp).IsRequired();

            return builder;
        }

        private static EntityTypeBuilder<T> AddTenantIdQueryFilter<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            // https://github.com/dotnet/efcore/pull/11017
            DbContext dummyDbContext = null;
            builder.HasQueryFilter(t => EF.Property<Guid>(t, MultiTenancy.TenantIdProp) == dummyDbContext.GetTenantIdFromContext());

            return builder;
        }
    }
}
