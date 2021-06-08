using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class ModelBuilderExtensions
    {
        public static void ConfigureMultiTenant(this ModelBuilder modelBuilder, DbContext dbContext)
        {
            if (!dbContext.IsSharedDatabase())
                return;

            var multiTenantEntities = modelBuilder.Model.GetEntityTypes().Where(entity => entity.IsMultiTenant());

            foreach (var entity in multiTenantEntities)
            {
                var multiTenantColumn = entity.FindAnnotation(MultiTenancy.MultiTenantColumnAnnotation)?.Value as string;
                var entityTypeBuilder = modelBuilder.GetTypedEntityBuilder(entity.ClrType);

                EntityTypeBuilderExtensions.AddTenantIdProperty((dynamic)entityTypeBuilder, multiTenantColumn);
                EntityTypeBuilderExtensions.AddTenantIdQueryFilter((dynamic)entityTypeBuilder);
            }
        }

        private static EntityTypeBuilder GetTypedEntityBuilder(this ModelBuilder modelBuilder, Type entityType)
        {
            var entityMi = modelBuilder.GetType()
                                       .GetMethods()
                                       .Where(m => m.Name == nameof(modelBuilder.Entity)
                                                   && m.IsGenericMethod
                                                   && m.ReturnType.IsGenericType
                                                   && typeof(EntityTypeBuilder).IsAssignableFrom(m.ReturnType))
                                       .Single()
                                       .MakeGenericMethod(entityType);

            var typedBuilder = entityMi.Invoke(modelBuilder, null);
            return typedBuilder as EntityTypeBuilder;
        }
    }
}
