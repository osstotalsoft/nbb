// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.EventStore.AdoNet.Internal;
using NBB.MultiTenancy.Abstractions.Context;
using System.Collections.Generic;
using System.Data;

namespace NBB.EventStore.AdoNet.Multitenancy
{
    public class AdoNetMultiTenantEventRepository : AdoNetEventRepository
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public AdoNetMultiTenantEventRepository(Scripts scripts, ILogger<AdoNetEventRepository> logger, IOptionsSnapshot<EventStoreAdoNetOptions> eventstoreOptions, ITenantContextAccessor tenantContextAccessor)
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
