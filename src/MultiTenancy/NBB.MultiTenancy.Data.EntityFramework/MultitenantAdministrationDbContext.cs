using Microsoft.EntityFrameworkCore;
using NBB.MultiTenancy.Data.EntityFramework.Entities;

namespace NBB.MultiTenancy.Data.EntityFramework
{
    public abstract class MultitenantAdministrationDbContext<T>: DbContext
    {
        public MultitenantAdministrationDbContext(DbContextOptions<MultitenantAdministrationDbContext<T>> options)
            : base(options)
        {
        }

        protected MultitenantAdministrationDbContext(DbContextOptions options) : base(options)
        {

        }

        public virtual DbSet<Feature<T>> Features { get; set; }
        public virtual DbSet<FeatureUserRight<T>> FeatureUserRights { get; set; }
        public virtual DbSet<Role<T>> Roles { get; set; }
        public virtual DbSet<RoleUserRight<T>> RoleUserRights { get; set; }
        public virtual DbSet<Subscription<T>> Subscriptions { get; set; }
        public virtual DbSet<SubscriptionFeature<T>> SubscriptionFeatures { get; set; }
        public virtual DbSet<Tenant<T>> Tenants { get; set; }
        public virtual DbSet<TenantFeature<T>> TenantFeatures { get; set; }
        public virtual DbSet<TenantSubcription<T>> TenantSubcriptions { get; set; }
        public virtual DbSet<TenantUser<T>> TenantUsers { get; set; }
        public virtual DbSet<User<T>> Users { get; set; }
        public virtual DbSet<UserFeature<T>> UserFeatures { get; set; }
        public virtual DbSet<UserRight<T>> UserRights { get; set; }
        public virtual DbSet<UserRole<T>> UserRoles { get; set; }
        

    }
}