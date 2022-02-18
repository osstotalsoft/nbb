// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Repositories
{
    public interface ITenantHostRepository
    {
        Task<Tenant> GetByHost(string host, CancellationToken token = default);
    }
}
