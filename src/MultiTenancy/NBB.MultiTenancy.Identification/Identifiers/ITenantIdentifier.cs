// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Identifiers
{
    public interface ITenantIdentifier
    {
        Task<Guid> GetTenantIdAsync(string tenantToken);
    }
}
