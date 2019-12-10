namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class UserFeature<T>
    {
        public T FeatureId { get; set; }
        public T UserId { get; set; }
        public decimal? FeatureValue { get; set; }

        public virtual Feature<T> Feature { get; set; }
        public virtual User<T> User { get; set; }
    }
}