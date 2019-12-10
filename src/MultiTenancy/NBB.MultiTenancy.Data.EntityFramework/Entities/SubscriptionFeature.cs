namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class SubscriptionFeature<T>
    {
        public T SubcriptionId { get; set; }
        public T FeatureId { get; set; }
        public decimal FeatureValue { get; set; }

        public virtual Feature<T> Feature { get; set; }
        public virtual Subscription<T> Subcription { get; set; }
    }
}