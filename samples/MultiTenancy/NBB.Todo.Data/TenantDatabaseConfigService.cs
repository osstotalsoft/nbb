using Microsoft.Extensions.Configuration;
using NBB.Data.EntityFramework.MultiTenancy;
using System;

namespace NBB.Todo.Data
{
    public class TenantDatabaseConfigService : ITenantDatabaseConfigService
    {
        private readonly IConfiguration _configuration;

        public TenantDatabaseConfigService(IConfiguration configuration)
            => _configuration = configuration;

        public string GetConnectionString(Guid tenantId)
            => _configuration.GetConnectionString("DefaultConnection");

        public bool IsSharedDatabase(Guid tenantId)
            => true;
    }
}
