namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class UserRole<T>
    {
        public T UserId { get; set; }
        public T RoleId { get; set; }
        public T TenantId { get; set; }

        public virtual Role<T> Role { get; set; }
        public virtual Tenant<T> Tenant { get; set; }
        public virtual User<T> User { get; set; }
    }
}