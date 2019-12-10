using System.Collections.Generic;

namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class UserRight<T>
    {
        public UserRight()
        {
            FeatureUserRights = new HashSet<FeatureUserRight<T>>();
            RoleUserRights = new HashSet<RoleUserRight<T>>();
        }

        public T UserRightId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual ICollection<FeatureUserRight<T>> FeatureUserRights { get; set; }
        public virtual ICollection<RoleUserRight<T>> RoleUserRights { get; set; }
    }
}