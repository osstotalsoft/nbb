using System;
using System.Collections.Generic;

namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class User<T>
    {
        public User()
        {
            TenantUsers = new HashSet<TenantUser<T>>();
            Tenants = new HashSet<Tenant<T>>();
            UserFeatures = new HashSet<UserFeature<T>>();
            UserRoles = new HashSet<UserRole<T>>();
        }

        public T UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Phone { get; set; }

        public virtual ICollection<TenantUser<T>> TenantUsers { get; set; }
        public virtual ICollection<Tenant<T>> Tenants { get; set; }
        public virtual ICollection<UserFeature<T>> UserFeatures { get; set; }
        public virtual ICollection<UserRole<T>> UserRoles { get; set; }
    }
}
