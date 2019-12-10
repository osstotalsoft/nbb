using System.Collections.Generic;

namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class Tenant<T>
    {
        public Tenant()
        {
            TenantFeatures = new HashSet<TenantFeature<T>>();
            TenantSubcriptions = new HashSet<TenantSubcription<T>>();
            TenantUsers = new HashSet<TenantUser<T>>();
            UserRoles = new HashSet<UserRole<T>>();
        }

        public T TenantId { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public string ConnectionString { get; set; }
        public int DatabaseClient { get; set; }
        public T OwnerId { get; set; }

        public virtual User<T> Owner { get; set; }
        public virtual ICollection<TenantFeature<T>> TenantFeatures { get; set; }
        public virtual ICollection<TenantSubcription<T>> TenantSubcriptions { get; set; }
        public virtual ICollection<TenantUser<T>> TenantUsers { get; set; }
        public virtual ICollection<UserRole<T>> UserRoles { get; set; }
    }
}