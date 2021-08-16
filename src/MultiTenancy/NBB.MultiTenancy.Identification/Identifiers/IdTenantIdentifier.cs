// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Identifiers
{
    public class IdTenantIdentifier : ITenantIdentifier
    {
        public Task<Guid> GetTenantIdAsync(string tenantToken)
        {
            var tenantId = Guid.Parse(tenantToken);
            return Task.FromResult(tenantId);
        }
    }
}
