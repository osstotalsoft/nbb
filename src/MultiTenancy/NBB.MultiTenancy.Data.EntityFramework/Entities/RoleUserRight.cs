namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class RoleUserRight<T>
    {
        public T RoleId { get; set; }
        public T UserRightId { get; set; }

        public virtual Role<T> Role { get; set; }
        public virtual UserRight<T> UserRight { get; set; }
    }
}