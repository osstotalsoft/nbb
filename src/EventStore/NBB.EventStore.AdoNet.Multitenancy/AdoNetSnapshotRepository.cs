using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.EventStore.Abstractions;
using NBB.EventStore.AdoNet.Internal;
using NBB.MultiTenancy.Abstractions.Context;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace NBB.EventStore.AdoNet.Multitenancy
{
    public class AdoNetMultitenantSnapshotRepository : AdoNetSnapshotRepository
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public AdoNetMultitenantSnapshotRepository(Scripts scripts,
            ILogger<AdoNetSnapshotRepository> logger, IOptions<EventStoreOptions> eventstoreOptions, ITenantContextAccessor tenantContextAccessor)
            : base(scripts, logger, eventstoreOptions)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }
        protected override IEnumerable<SqlParameter> GetGlobalFilterParams()
        {
            var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();
            yield return new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { Value = tenantId };
        }
    }
}
