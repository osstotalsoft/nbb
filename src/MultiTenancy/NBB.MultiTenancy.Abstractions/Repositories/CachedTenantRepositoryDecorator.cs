// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Repositories
{
    public class CachedTenantRepositoryDecorator : ITenantRepository
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IDistributedCache _cache;
        private static readonly Func<Guid, string> CacheTenantByIdKey = tenantId => $"tenantId:{tenantId}";
        private static readonly Func<string, string> CacheTenantByHostKey = host => $"tenantHost:{host}";
        private static readonly string AllKey = "__ALL__Tenants";

        public CachedTenantRepositoryDecorator(ITenantRepository tenantRepository, IDistributedCache cache)
        {
            _tenantRepository = tenantRepository;
            _cache = cache;
        }

        public async Task<Tenant> Get(Guid id, CancellationToken token)
        {
            var cacheKey = CacheTenantByIdKey(id);
            var cachedTenant = await GetTenantFromCache(cacheKey, token);
            if (cachedTenant != null)
            {
                return cachedTenant;
            }
                        
            var dbTenant = await _tenantRepository.Get(id, token);
            if (dbTenant == null)
            {
                return null;
            }

            await SetTenantToCache(dbTenant, cacheKey, token);

            return dbTenant;
        }

        private async Task<Tenant> GetTenantFromCache(string key, CancellationToken token = default)
        {
            var sTenant = await _cache.GetStringAsync(key, token);
            return string.IsNullOrWhiteSpace(sTenant) ? null : JsonConvert.DeserializeObject<Tenant>(sTenant);
        }

        private async Task SetTenantToCache(Tenant tenant, string key, CancellationToken token = default)
        {
            var sTenant = JsonConvert.SerializeObject(tenant);
            await _cache.SetStringAsync(key, sTenant,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                },token);
        }

        public Task<List<Tenant>> GetAll(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        private async Task<List<Tenant>> GetAllTenantsFromCache(CancellationToken token = default)
        {
            var list = await _cache.GetStringAsync(AllKey, token);
            return string.IsNullOrWhiteSpace(list) ? null : JsonConvert.DeserializeObject<List<Tenant>>(list);
        }

        private async Task SetAllTenantsToCache(List<Tenant> tenants, CancellationToken token = default)
        {
            var list = JsonConvert.SerializeObject(tenants);
            await _cache.SetStringAsync(AllKey, list,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                }, token);
        }
    }
}
