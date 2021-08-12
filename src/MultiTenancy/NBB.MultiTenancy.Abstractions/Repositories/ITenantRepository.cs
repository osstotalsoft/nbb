// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant> Get(Guid id, CancellationToken token = default);
        Task<Tenant> GetByHost(string host, CancellationToken token = default);
    }
}
