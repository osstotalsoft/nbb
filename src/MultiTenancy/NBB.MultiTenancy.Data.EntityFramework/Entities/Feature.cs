using System.Collections.Generic;

namespace NBB.MultiTenancy.Data.EntityFramework.Entities
{
    public partial class Feature<T>
    {
        public Feature()
        {
            FeatureUserRights = new HashSet<FeatureUserRight<T>>();
            SubscriptionFeatures = new HashSet<SubscriptionFeature<T>>();
            TenantFeatures = new HashSet<TenantFeature<T>>();
            UserFeatures = new HashSet<UserFeature<T>>();
        }

        public T FeatureId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<FeatureUserRight<T>> FeatureUserRights { get; set; }
        public virtual ICollection<SubscriptionFeature<T>> SubscriptionFeatures { get; set; }
        public virtual ICollection<TenantFeature<T>> TenantFeatures { get; set; }
        public virtual ICollection<UserFeature<T>> UserFeatures { get; set; }
    }
}