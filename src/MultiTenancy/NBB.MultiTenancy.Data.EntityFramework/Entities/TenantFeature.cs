namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class TenantFeature<T>
    {
        public T TenantId { get; set; }
        public T FeatureId { get; set; }
        public decimal? FeatureValue { get; set; }

        public virtual Feature<T> Feature { get; set; }
        public virtual Tenant<T> Tenant { get; set; }
    }
}