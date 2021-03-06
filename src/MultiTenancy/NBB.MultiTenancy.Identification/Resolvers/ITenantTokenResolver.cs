﻿using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Resolvers
{
    public interface ITenantTokenResolver
    {
        /// <summary>
        /// Gets tenant identification token form a context
        /// </summary>
        /// <returns>Tenant token</returns>
        Task<string> GetTenantToken();
    }
}
