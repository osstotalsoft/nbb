using System.Collections.Generic;

namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class Role<T>
    {
        public Role()
        {
            RoleUserRights = new HashSet<RoleUserRight<T>>();
            UserRoles = new HashSet<UserRole<T>>();
        }

        public T RoleId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual ICollection<RoleUserRight<T>> RoleUserRights { get; set; }
        public virtual ICollection<UserRole<T>> UserRoles { get; set; }
    }
}