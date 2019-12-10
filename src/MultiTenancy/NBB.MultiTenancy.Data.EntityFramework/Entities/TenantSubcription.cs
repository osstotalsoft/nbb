using System;


namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class TenantSubcription<T>
    {
        public T TenantId { get; set; }
        public T SubcriptionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual Subscription<T> Subcription { get; set; }
        public virtual Tenant<T> Tenant { get; set; }
    }
}
