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

        public async Task<Tenant> TryGet(Guid id, CancellationToken token)
        {
            var cacheKey = CacheTenantByIdKey(id);
            var cachedTenant = await GetTenantFromCache(cacheKey, token);
            if (cachedTenant != null)
            {
                return cachedTenant;
            }

            var dbTenant = await _tenantRepository.TryGet(id, token);
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

        public async Task<List<Tenant>> GetAll(CancellationToken token = default)
        {
            var tenants = await _tenantRepository.GetAll(token);
            return tenants;
        }

        public async Task<Tenant> GetByHost(string host, CancellationToken token)
        {
            var cacheKey = CacheTenantByHostKey(host);
            var cachedTenant = await GetTenantFromCache(cacheKey, token);
            if (cachedTenant != null)
            {
                return cachedTenant;
            }

            var dbTenant = await _tenantRepository.GetByHost(host, token);
            if (dbTenant == null)
            {
                return null;
            }

            await SetTenantToCache(dbTenant, cacheKey, token);

            return dbTenant;
        }
    }
}
